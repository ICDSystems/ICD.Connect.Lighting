namespace ICD.Connect.Lighting.Mock.Controls
{
	public sealed class MockLightingPresetControl : AbstractMockLightingControl
	{
		public override LightingProcessorControl.eControlType ControlType
		{
			get { return LightingProcessorControl.eControlType.Preset; }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		public MockLightingPresetControl(int id, int room, string name)
			: base(id, room, name)
		{
		}
	}
}
