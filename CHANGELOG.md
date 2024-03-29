# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [8.0.2] - 2023-03-23
### Changed
 - Fixed XML generation for ShadeType in AbstractShadeDeviceSettings

## [8.0.1] - 2023-03-22
###Changed
 - Removed Obfuscation

## [8.0.0] - 2022-09-23
### Changed
 - Refactored shade last direction interfaces to be more consistent with other shade controls

## [7.1.2] - 2022-07-01
### Changed
 - Updated Crestron SDK to 2.18.96

## [7.1.1] - 2022-03-23
### Added
 - Added SerialLightingRoomInterfaceDevice for basic one way serial control of lighting systems

### Changed
 - Added controls to ShadeGroup - Stop, SetPosition, InMotionFeedback, LastDirection

## [7.1.0] - 2021-10-04
### Changed
 - Changed LightingRoomInterfaceOccupancyControl to implement new IOccupancySensorControl elements

## [7.0.1] - 2021-07-26
### Changed
 - Added default network and comspec for Lutron devices and LightingProcessorClient

## [7.0.0] - 2021-05-14
### Added
 - Added support for Lutron Grafik Eye lights/shades
 
### Changed
 - Refactor of Lutron integrations to support Graifk Eye integration better

## [6.1.0] - 2021-01-14
### Added
 - LightingProcessorServer - UseSecureServer setting when set to true will use IcdSecureTcpServer otherwise IcdTcpServer will be used.
 - LightingProcessorServer - Added default values for UseSecureServer (false) and MaxClients (32)
 - Added CrestronSPlus Lighting Project
 - Added LightingRoomInterfaceShim

### Changed
 - LightingProcessorClient - Fixed issues around intialization - RoomId is now stored discretely by the client
 - LightingProcessorClient - Added console commands for Connect/Disconnect RPC, added console access to lighting room object
 - LightingProcessorClient - Client now configures the port correctly
 - MockLightingRoom - Added presets access to console
 - AbstractLutronNwkDevice - fixed issue where commands weren't being sent after the first command

### Changed
 - Applied Cresnet settings and console improvments to CsmQmt50DccnAdapter
 - Fixed a bug where LightingProcessorClientDevice was not being initialized with a room id in the correct order

## [6.0.0] - 2020-06-19
### Changed
 - MockLightingProcessor now implements IMockDevice
 - MockShadeDevice now implement IMockDevice
 - Using new logging context
 - Occupancy features moved to ICD.Connect.Partitioning

## [5.3.0] - 2020-10-06
### Changed
 - Implemented StartSettings to start communications with devices

## [5.2.2] - 2020-08-03
### Changed
 - EiscLightingAdapterDevice - changed to not blow up if configuration doesn't include presets/loads/shades
 - LightingProcessorServer - changed configuration of shade groups to be in consisten parent tag

## [5.2.1] - 2020-06-19
### Changed
 - AbstractLightingRoomInterface - changed default for EnableOccupancyControl to true for backwards compatibility

## [5.2.0] - 2020-06-02
### Added
 - LightingProcessorServer - added console commands to print clients for rooms

## [5.1.0] - 2020-05-01
### Added
 - Added support for local Occupancy sensors on LightingProcessorServer
 - Added AbstractLightingRoomInterfaceDevice with option OccupancySensorControl
 
### Changed
 - Changed LightingProcessorServer to support configurations without an ILightingProcessor configured
 - Fixed LightingProcessorClient to correctly configure the attached port
 - LightingProcessorClient and LightingRoomInterfaceDevice now inherit from AbstractLightingRoomInterfaceDevice

## [5.0.1] - 2020-04-05
### Changed
 - Fixed issue where Crestron shades wouldn't update online status correctly
 - Fixed issue where Shade Groups wouldn't update online status correctly

## [5.0.0] - 2020-03-20
### Added
 - Support for LutronNwkDevice, for non-Quantum Lutron systems
 - ILightingRoomInterfaceDevice, device for a single room to interface with a ILightingProcessorDevice
 - LightingRoomInterfaceDevice, simple implementation of the ILightingRoomInterfaceDevice interface
 - ILightingRoomInterfaceDevice extensions
 
### Changed
 - LightingProcessorClientDevice now implements ILightingRoomInterfaceDevice instead of ILightingProcessorDevice
 - Massive refactor of LutronQuantumNwkDevice to abstract common elemnets with LutronNwkDevice

## [4.1.0] - 2019-10-07
### Changed
 - Using new GenericBaseUtils to standardize crestron device setup and teardown
 - Using CresnetSettingsUtils to standardize cresnet settings serialization

## [4.0.0] - 2019-01-10
### Added
 - Added port configuration features to lighting devices

## [3.4.0] - 2020-05-05
### Added
 - Added Occupancy Control for LightingProcessorClient

## [3.3.1] - 2020-03-06
### Changed
 - EiscLightingAdapterDevice - Unregister EISC when disposing device

## [3.3.0] - 2020-02-19
### Added
 - Added Shade controls to be configurable on the EiscLightingAdapterDevice, with EISC joins for open, close, and stop

## [3.2.0] - 2020-02-03
### Added
 - Relay Shade Device - provides shade control with open/close relays
 - Added EiscLightingAdapterDevice to interface with D3 lighting programs from Krang

## [3.1.6] - 2019-11-18
### Changed
 - Removed ConnectionStateManager from Lighting Client since it's included in RPC Client

## [3.1.5] - 2019-05-20
### Changed
 - LightingServer online state is based on the TCP server listen state

## [3.1.4] - 2019-05-16
### Changed
 - Ensuring sigs are not nullsigs before getting values

## [3.1.3] - 2019-03-19
### Changed
 - Hide LightingProcessorServer settings properties that can't be rendered on the touch panel

## [3.1.2] - 2018-10-30
### Changed
 - Fixed loading issue where devices would not fail gracefully when a port was nto available

## [3.1.1] - 2018-07-02
### Changed
 - Lazy load room Ids for convienence
 - Set room id in apply settings first, to prevent the creation of a null room

## [3.1.0] - 2018-06-04
### Changed
 - Using ConnectionStateManager to maintain serial device connection

## [3.0.1] - 2018-05-09
### Changed
 - Fixed potential issues with loading configs in UTF8-BOM encoding

## [3.0.0] - 2018-04-24
### Changed
 - Changed all lighting pieces to use eOccupancyState from ICD.Connect.Misc.Occupancy
 - Removed suffix from assembly names
 - Using API event args
