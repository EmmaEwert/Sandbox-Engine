using UnityEngine;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(menuName = "Rendering/Pipeline")]
public class PipelineAsset : RenderPipelineAsset {
	[SerializeField] public bool dynamicBatching;
	[SerializeField] public bool instancing;

	protected override IRenderPipeline InternalCreatePipeline() {
		return new Pipeline(dynamicBatching, instancing);
	}
}
