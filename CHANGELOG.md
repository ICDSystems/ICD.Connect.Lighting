# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
