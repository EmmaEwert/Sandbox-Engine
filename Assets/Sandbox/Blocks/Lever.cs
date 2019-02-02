namespace Sandbox {
	using Sandbox.Core;

	public class Lever : Block {
		public override bool opaqueCube => false;

		public Lever() : base("lever", "face=floor,facing=north,powered=false") { }
	}
}