using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class Pipeline : RenderPipeline {
	private const int maxVisibleLights = 64;
	
	private static int visibleLightColorsId = Shader.PropertyToID("_VisibleLightColors");
	private static int visibleLightDirectionsOrPositionsId = Shader.PropertyToID("_VisibleLightDirectionsOrPositions");
	private static int visibleLightAttenuationsId = Shader.PropertyToID("_VisibleLightAttenuations");
	private static int visibleLightSpotDirectionsId = Shader.PropertyToID("_VisibleLightSpotDirections");
	private static int lightIndicesOffsetAndCountID = Shader.PropertyToID("unity_LightIndicesOffsetAndCount");
	
	private Vector4[] visibleLightColors = new Vector4[maxVisibleLights];
	private Vector4[] visibleLightDirectionsOrPositions = new Vector4[maxVisibleLights];
	private Vector4[] visibleLightAttenuations = new Vector4[maxVisibleLights];
	private Vector4[] visibleLightSpotDirections = new Vector4[maxVisibleLights];

	private CullResults cull;
	private CommandBuffer cameraBuffer = new CommandBuffer { name = "Render Camera" };
	private DrawRendererFlags drawFlags;
	private Material _errorMaterial;
	private Material errorMaterial => _errorMaterial = _errorMaterial ??
		new Material(Shader.Find("Hidden/InternalErrorShader")) {
			hideFlags = HideFlags.HideAndDontSave
		};

	public Pipeline(bool dynamicBatching, bool instancing) {
		if (dynamicBatching) {
			drawFlags = DrawRendererFlags.EnableDynamicBatching;
		}
		if (instancing) {
			drawFlags |= DrawRendererFlags.EnableInstancing;
		}
	}

	public override void Render(ScriptableRenderContext context, Camera[] cameras) {
		base.Render(context, cameras);
		foreach (var camera in cameras) {
			Render(context, camera);
		}
	}

	private void Render(ScriptableRenderContext context, Camera camera) {
		if (!CullResults.GetCullingParameters(camera, out var cullingParameters)) {
			return;
		}

#if UNITY_EDITOR
		if (camera.cameraType == CameraType.SceneView) {
			ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
		}
#endif // UNITY_EDITOR

		CullResults.Cull(ref cullingParameters, context, ref cull);

		context.SetupCameraProperties(camera);

		var clearFlags = camera.clearFlags;
		cameraBuffer.ClearRenderTarget(
			(clearFlags & CameraClearFlags.Depth) != 0,
			(clearFlags & CameraClearFlags.Color) != 0,
			camera.backgroundColor
		);

		if (cull.visibleLights.Count > 0) {
			ConfigureLights();
		} else {
			cameraBuffer.SetGlobalVector(lightIndicesOffsetAndCountID, Vector4.zero);
		}

		cameraBuffer.BeginSample("Render Camera");
		cameraBuffer.SetGlobalVectorArray(visibleLightColorsId, visibleLightColors);
		cameraBuffer.SetGlobalVectorArray(visibleLightDirectionsOrPositionsId, visibleLightDirectionsOrPositions);
		cameraBuffer.SetGlobalVectorArray(visibleLightAttenuationsId, visibleLightAttenuations);
		cameraBuffer.SetGlobalVectorArray(visibleLightSpotDirectionsId, visibleLightSpotDirections);
		context.ExecuteCommandBuffer(cameraBuffer);
		cameraBuffer.Clear();

		var drawSettings = new DrawRendererSettings(camera, new ShaderPassName("SRPDefaultUnlit"));
		if (cull.visibleLights.Count > 0) {
			drawSettings.rendererConfiguration = RendererConfiguration.PerObjectLightIndices8;
		}
		drawSettings.flags = drawFlags;
		drawSettings.sorting.flags = SortFlags.CommonOpaque;
		var filterSettings = new FilterRenderersSettings(initializeValues: true) {
			renderQueueRange = RenderQueueRange.opaque
		};
		context.DrawRenderers(
			cull.visibleRenderers, ref drawSettings, filterSettings
		);

		context.DrawSkybox(camera);

		drawSettings.sorting.flags = SortFlags.CommonTransparent;
		filterSettings.renderQueueRange = RenderQueueRange.transparent;
		context.DrawRenderers(
			cull.visibleRenderers, ref drawSettings, filterSettings
		);

		DrawDefaultPipeline(context, camera);

		cameraBuffer.EndSample("Render Camera");
		context.ExecuteCommandBuffer(cameraBuffer);
		cameraBuffer.Clear();

		context.Submit();
	}

	private void ConfigureLights () {
		for (int i = 0; i < cull.visibleLights.Count; ++i) {
			if (i == maxVisibleLights) { break; }
			var light = cull.visibleLights[i];
			visibleLightColors[i] = light.finalColor;
			var attenuation = Vector4.zero;
			attenuation.w = 1f;
			if (light.lightType == LightType.Directional) {
				visibleLightDirectionsOrPositions[i] = -light.localToWorld.GetColumn(2);
			} else {
				visibleLightDirectionsOrPositions[i] = light.localToWorld.GetColumn(3);
				attenuation.x = 1f / Mathf.Max(light.range * light.range, 0.00001f);
				if (light.lightType == LightType.Spot) {
					visibleLightSpotDirections[i] = -light.localToWorld.GetColumn(2);
					var outerRad = Mathf.Deg2Rad * 0.5f * light.spotAngle;
					var outerCos = Mathf.Cos(outerRad);
					var outerTan = Mathf.Tan(outerRad);
					var innerCos = Mathf.Cos(Mathf.Atan(((46f / 64f) * outerTan)));
					var angleRange = Mathf.Max(innerCos - outerCos, 0.001f);
					attenuation.z = 1f / angleRange;
					attenuation.w = -outerCos * attenuation.z;
				}
			}
			visibleLightAttenuations[i] = attenuation;
		}

		if (cull.visibleLights.Count > maxVisibleLights) {
			var lightIndices = cull.GetLightIndexMap();
			for (var i = maxVisibleLights; i < cull.visibleLights.Count; i++) {
				lightIndices[i] = -1;
			}
			cull.SetLightIndexMap(lightIndices);
		}
	}

	[Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
	private void DrawDefaultPipeline(ScriptableRenderContext context, Camera camera) {
		var drawSettings = new DrawRendererSettings(camera, new ShaderPassName("ForwardBase"));
		drawSettings.SetShaderPassName(1, new ShaderPassName("PrepassBase"));
		drawSettings.SetShaderPassName(2, new ShaderPassName("Always"));
		drawSettings.SetShaderPassName(3, new ShaderPassName("Vertex"));
		drawSettings.SetShaderPassName(4, new ShaderPassName("VertexLMRGBM"));
		drawSettings.SetShaderPassName(5, new ShaderPassName("VertexLM"));
		drawSettings.SetOverrideMaterial(errorMaterial, passIndex: 0);
		var filterSettings = new FilterRenderersSettings(initializeValues: true);

		context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);
	}
}
