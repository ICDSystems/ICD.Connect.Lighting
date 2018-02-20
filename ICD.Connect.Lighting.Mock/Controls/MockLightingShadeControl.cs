namespace ICD.Connect.Lighting.Mock.Controls
{
	public sealed class MockLightingShadeControl : AbstractMockShadeLightingControl
	{
		public override LightingProcessorControl.eControlType ControlType
		{
			get { return LightingProcessorControl.eControlType.Shade; }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		public MockLightingShadeControl(int id, int room, string name)
			: base(id, room, name)
		{
		}

		public override void Dispose()
		{
			
		}
	}
}
