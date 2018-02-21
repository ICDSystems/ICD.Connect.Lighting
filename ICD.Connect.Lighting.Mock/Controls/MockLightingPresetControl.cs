using ICD.Connect.Lighting.Environment;

namespace ICD.Connect.Lighting.Mock.Controls
{
	public sealed class MockLightingPresetControl : AbstractMockLightingControl, IPresetEnvironmentPeripheral
	{
		public override LightingProcessorControl.ePeripheralType PeripheralType
		{
			get { return LightingProcessorControl.ePeripheralType.Preset; }
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

		public override void Dispose()
		{
			
		}
	}
}
