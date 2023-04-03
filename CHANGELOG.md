## [3.37.1](https://github.com/informatievlaanderen/road-registry/compare/v3.37.0...v3.37.1) (2023-03-22)


### Bug Fixes

* don't update RecordingDate with ModifyRoadSegment ([#902](https://github.com/informatievlaanderen/road-registry/issues/902)) ([843f1ab](https://github.com/informatievlaanderen/road-registry/commit/843f1abd5089130b69d66ec72479857a3fea9ed0))
* WR-645 add Outlined validation rules to file upload ([#901](https://github.com/informatievlaanderen/road-registry/issues/901)) ([a420315](https://github.com/informatievlaanderen/road-registry/commit/a420315bce87195e3081831731373b5c3f0fb7e0))

# [3.37.0](https://github.com/informatievlaanderen/road-registry/compare/v3.36.9...v3.37.0) (2023-03-20)


### Bug Fixes

* WR-644 correct field sizes after FC changes ([#900](https://github.com/informatievlaanderen/road-registry/issues/900)) ([59462d8](https://github.com/informatievlaanderen/road-registry/commit/59462d8edf8eb8c1178e817fd522ebb6f5bf1d79))


### Features

* WR-586 roadsegment edit attributes endpoint ([#898](https://github.com/informatievlaanderen/road-registry/issues/898)) ([a71985b](https://github.com/informatievlaanderen/road-registry/commit/a71985b3247a6a7e442130a9c8a66074dd6d7991))

## [3.36.9](https://github.com/informatievlaanderen/road-registry/compare/v3.36.8...v3.36.9) (2023-03-15)


### Bug Fixes

* lambda graceful shutdown to avoid open locks ([#890](https://github.com/informatievlaanderen/road-registry/issues/890)) ([f8b8ac8](https://github.com/informatievlaanderen/road-registry/commit/f8b8ac8d442f5e4400cd97b179769ef6bbcf1b71))
* return upload validation as consistent response (including error codes) ([#893](https://github.com/informatievlaanderen/road-registry/issues/893)) ([c2ceb7a](https://github.com/informatievlaanderen/road-registry/commit/c2ceb7afc7e3963e9a007afafe8a79910172e480))
* WR-640 handle upload after FC by commandhost instead of API ([#895](https://github.com/informatievlaanderen/road-registry/issues/895)) ([7ea194f](https://github.com/informatievlaanderen/road-registry/commit/7ea194f8aba9634e68a6efe63e1dd2a71ce840e1))

## [3.36.8](https://github.com/informatievlaanderen/road-registry/compare/v3.36.7...v3.36.8) (2023-03-13)


### Bug Fixes

* WR-509 add missing validation road segment lane direction ([#889](https://github.com/informatievlaanderen/road-registry/issues/889)) ([f20bf5a](https://github.com/informatievlaanderen/road-registry/commit/f20bf5a0759c0cbf5bfc30a8de4133862e57b0dd))

## [3.36.7](https://github.com/informatievlaanderen/road-registry/compare/v3.36.6...v3.36.7) (2023-03-09)


### Bug Fixes

* WR-623 link/unlink streetname must have at least left or right streetnameid filled in ([#888](https://github.com/informatievlaanderen/road-registry/issues/888)) ([4301b79](https://github.com/informatievlaanderen/road-registry/commit/4301b79c5c02b42b48941601c8f83798649f8063))

## [3.36.6](https://github.com/informatievlaanderen/road-registry/compare/v3.36.5...v3.36.6) (2023-03-08)


### Bug Fixes

* add version property to RoadNode & RoadSegment Kafka projections ([#887](https://github.com/informatievlaanderen/road-registry/issues/887)) ([d85b56b](https://github.com/informatievlaanderen/road-registry/commit/d85b56bf021acad011db54162cae4d56d41396a8))

## [3.36.5](https://github.com/informatievlaanderen/road-registry/compare/v3.36.4...v3.36.5) (2023-03-08)


### Bug Fixes

* nuget download/upload ([#886](https://github.com/informatievlaanderen/road-registry/issues/886)) ([e95ad8e](https://github.com/informatievlaanderen/road-registry/commit/e95ad8ea511e041f9c398afa3efeaf8e9259a548))

## [3.36.4](https://github.com/informatievlaanderen/road-registry/compare/v3.36.3...v3.36.4) (2023-03-07)


### Bug Fixes

* cleanup unused roadnetwork data in memory ([#885](https://github.com/informatievlaanderen/road-registry/issues/885)) ([8e6b196](https://github.com/informatievlaanderen/road-registry/commit/8e6b196982596279d6f2f1a73aa1b53d3e5e99ca))

## [3.36.3](https://github.com/informatievlaanderen/road-registry/compare/v3.36.2...v3.36.3) (2023-03-07)


### Bug Fixes

* release pipeline - add step upload NuGet package BackOffice ([87e8ca5](https://github.com/informatievlaanderen/road-registry/commit/87e8ca5a418f76863d0d662031d6dad8402b132e))

## [3.36.2](https://github.com/informatievlaanderen/road-registry/compare/v3.36.1...v3.36.2) (2023-03-07)


### Bug Fixes

* add Backoffice lib to build.fsx ([#880](https://github.com/informatievlaanderen/road-registry/issues/880)) ([eebd22d](https://github.com/informatievlaanderen/road-registry/commit/eebd22def2ff5d76b0330486fad241e5fa2971d9))
* add paket.template for RoadRegistry.BackOffice ([#879](https://github.com/informatievlaanderen/road-registry/issues/879)) ([2f8cc65](https://github.com/informatievlaanderen/road-registry/commit/2f8cc65a4156f7dbd075825f9696cf3c76186d0a))
* build backoffice nuget ([#881](https://github.com/informatievlaanderen/road-registry/issues/881)) ([447e86e](https://github.com/informatievlaanderen/road-registry/commit/447e86e29a1b949e25cbe30dc40afd936295092d))
* don't increment streamversion when loading snapshot ([#882](https://github.com/informatievlaanderen/road-registry/issues/882)) ([77b418d](https://github.com/informatievlaanderen/road-registry/commit/77b418da31e18c2491329ae7dc15fe79e4600483))

## [3.36.1](https://github.com/informatievlaanderen/road-registry/compare/v3.36.0...v3.36.1) (2023-03-03)


### Bug Fixes

* unescape data property in KafkaJsonMessage ([8017768](https://github.com/informatievlaanderen/road-registry/commit/8017768f53ff5135a60b2844e7545f8c8887f0d3))

# [3.36.0](https://github.com/informatievlaanderen/road-registry/compare/v3.35.5...v3.36.0) (2023-03-03)


### Bug Fixes

* add localstack for SQS local development ([#876](https://github.com/informatievlaanderen/road-registry/issues/876)) ([daea055](https://github.com/informatievlaanderen/road-registry/commit/daea05503dc61666ac8981925d396af6df32f95e))
* enable/disable projections in projector ([#870](https://github.com/informatievlaanderen/road-registry/issues/870)) ([f752a37](https://github.com/informatievlaanderen/road-registry/commit/f752a37f45b22d050c5c2d1229c147eb1b15621d))


### Features

* Paket reference & template update ([#877](https://github.com/informatievlaanderen/road-registry/issues/877)) ([e51f64d](https://github.com/informatievlaanderen/road-registry/commit/e51f64d7e18cf7eb5cc8e679e3fc5f0446cee4a6))

## [3.35.5](https://github.com/informatievlaanderen/road-registry/compare/v3.35.4...v3.35.5) (2023-03-02)


### Bug Fixes

* messaginghost-sqs only register command to store, do not handle message ([#875](https://github.com/informatievlaanderen/road-registry/issues/875)) ([04f0b95](https://github.com/informatievlaanderen/road-registry/commit/04f0b95528935eddf6879803140e958c318006cf))

## [3.35.4](https://github.com/informatievlaanderen/road-registry/compare/v3.35.3...v3.35.4) (2023-03-02)


### Bug Fixes

* bump mediatr ([0f6c75f](https://github.com/informatievlaanderen/road-registry/commit/0f6c75fcf93e6e0d4ff4f107722711cf2217ec46))
* kafka producer snapshot bugfix ([#873](https://github.com/informatievlaanderen/road-registry/issues/873)) ([b75c1db](https://github.com/informatievlaanderen/road-registry/commit/b75c1db2f1ff9a13605dae09cda747c25596b4f8))
* Paket template update ([#871](https://github.com/informatievlaanderen/road-registry/issues/871)) ([097e46b](https://github.com/informatievlaanderen/road-registry/commit/097e46b21bf5c36805a963b11bf2976070eead2e))
* paket.template API include dlls ([#872](https://github.com/informatievlaanderen/road-registry/issues/872)) ([c70f00c](https://github.com/informatievlaanderen/road-registry/commit/c70f00c4d9f66ddfd901e5509b43dd4bbdf82221))
* skip 2 tests ([53f19c4](https://github.com/informatievlaanderen/road-registry/commit/53f19c45fc1193b5d8562f9502060eaf4d53be09))
* skip test ([feb0027](https://github.com/informatievlaanderen/road-registry/commit/feb0027f93e8f313b741d163222e7371d7a3e18d))
* Updated paket.template file for ZipArchiveWriters ([#874](https://github.com/informatievlaanderen/road-registry/issues/874)) ([36e4bd2](https://github.com/informatievlaanderen/road-registry/commit/36e4bd2295dec48b9df1579b3e5ad152849e7a25))
* WR-560 snapshot s3 + rebuild snapshot ([#868](https://github.com/informatievlaanderen/road-registry/issues/868)) ([fdcbb4f](https://github.com/informatievlaanderen/road-registry/commit/fdcbb4fb55412034e0c01daf077d6746a6f9b830))
* WR-609 set streetnameid to NotApplicable when unlinking + fix disabled tests ([#869](https://github.com/informatievlaanderen/road-registry/issues/869)) ([5a3ccad](https://github.com/informatievlaanderen/road-registry/commit/5a3ccad4ee60d1970db784930e54b9e84014e292))

## [3.35.3](https://github.com/informatievlaanderen/road-registry/compare/v3.35.2...v3.35.3) (2023-02-27)


### Bug Fixes

* WR-608 disconnect nodes from segment (in the event) when start/end node changed ([#865](https://github.com/informatievlaanderen/road-registry/issues/865)) ([ff05fe4](https://github.com/informatievlaanderen/road-registry/commit/ff05fe40a43e7fd5169f70016e2c11baa7313de2))

## [3.35.2](https://github.com/informatievlaanderen/road-registry/compare/v3.35.1...v3.35.2) (2023-02-27)


### Bug Fixes

* Missing registration for distributed S3 cache ([#864](https://github.com/informatievlaanderen/road-registry/issues/864)) ([82868bf](https://github.com/informatievlaanderen/road-registry/commit/82868bf550e658412be9ec6ae62400039d8853b5))

## [3.35.1](https://github.com/informatievlaanderen/road-registry/compare/v3.35.0...v3.35.1) (2023-02-24)


### Bug Fixes

* remove dev config from production Lambdas appsettings ([#863](https://github.com/informatievlaanderen/road-registry/issues/863)) ([b4a9dba](https://github.com/informatievlaanderen/road-registry/commit/b4a9dba4c9c8845aa5bc603be0e3e8f1d6f05599))
* stop autorefresh when switching page ([#861](https://github.com/informatievlaanderen/road-registry/issues/861)) ([33f67f4](https://github.com/informatievlaanderen/road-registry/commit/33f67f41586a6b2a0d628f4b458adc818f936e4a))
* streetname consumer set to app instead of library ([73cf09d](https://github.com/informatievlaanderen/road-registry/commit/73cf09db99a0a2324d2f860ca41ea6561eabd7ea))

# [3.35.0](https://github.com/informatievlaanderen/road-registry/compare/v3.34.6...v3.35.0) (2023-02-24)


### Bug Fixes

* extra info in exceptions when replaying store messages fails ([#844](https://github.com/informatievlaanderen/road-registry/issues/844)) ([add21b6](https://github.com/informatievlaanderen/road-registry/commit/add21b6abf165eb3ef1d44e80ac15f102d6f2e74))
* Paket template fixes ([#859](https://github.com/informatievlaanderen/road-registry/issues/859)) ([a574306](https://github.com/informatievlaanderen/road-registry/commit/a574306eb5b3d019beea0c6d8e09c3accb46dee0))
* Publish output files should not throw an error ([#857](https://github.com/informatievlaanderen/road-registry/issues/857)) ([3f4bbe6](https://github.com/informatievlaanderen/road-registry/commit/3f4bbe63a0f3467f67e9c81e57902fecac485b26))
* WR-508 delete S3 item after SQS message processed ([#842](https://github.com/informatievlaanderen/road-registry/issues/842)) ([372750e](https://github.com/informatievlaanderen/road-registry/commit/372750ed39ef83967e1dd37e9e160c4082a819bc))
* WR-600 streetname link/unlink ability to do both left and right in 1 request ([#856](https://github.com/informatievlaanderen/road-registry/issues/856)) ([8983606](https://github.com/informatievlaanderen/road-registry/commit/898360606f96b6ac2886212a5cdaf9530e3fd525))
* WR-607 updating roadsegment start/end node must happen at once to avoid them being the same ([#858](https://github.com/informatievlaanderen/road-registry/issues/858)) ([9cadbb2](https://github.com/informatievlaanderen/road-registry/commit/9cadbb2607229908887deee774fa181635cef199))


### Features

* WR-529 delete road segment outline ([#845](https://github.com/informatievlaanderen/road-registry/issues/845)) ([4f61feb](https://github.com/informatievlaanderen/road-registry/commit/4f61feb7422743a02ed5f895669e3bfe6fa3afaf))
* WR-560 add snapshot lambda function ([#843](https://github.com/informatievlaanderen/road-registry/issues/843)) ([de3968e](https://github.com/informatievlaanderen/road-registry/commit/de3968ec8639fc9a3ce563d017038475381659a3))
* WR-585 Snapshot reader and writer use of distributed S3 cache ([#855](https://github.com/informatievlaanderen/road-registry/issues/855)) ([769ff17](https://github.com/informatievlaanderen/road-registry/commit/769ff173ee8f7512ff78f2cd430effea91a3afa3))

## [3.34.6](https://github.com/informatievlaanderen/road-registry/compare/v3.34.5...v3.34.6) (2023-02-14)


### Bug Fixes

* Upload dropbox tekst naar huidige situatie ([#841](https://github.com/informatievlaanderen/road-registry/issues/841)) ([e597521](https://github.com/informatievlaanderen/road-registry/commit/e597521a5ff982ce6e683878bde5e513d4d9822b))

## [3.34.5](https://github.com/informatievlaanderen/road-registry/compare/v3.34.4...v3.34.5) (2023-02-14)


### Bug Fixes

* lambda env for deployment ([#839](https://github.com/informatievlaanderen/road-registry/issues/839)) ([f78f28b](https://github.com/informatievlaanderen/road-registry/commit/f78f28b0976e348b23c08529d60f18e385abce20))
* use S3 for big SQS messages ([#838](https://github.com/informatievlaanderen/road-registry/issues/838)) ([d6de33d](https://github.com/informatievlaanderen/road-registry/commit/d6de33d58d6cc3480043175ebbeaca915b704b94))

## [3.34.4](https://github.com/informatievlaanderen/road-registry/compare/v3.34.3...v3.34.4) (2023-02-13)


### Bug Fixes

* WR-449 lock stream when doing road network changes ([#833](https://github.com/informatievlaanderen/road-registry/issues/833)) ([b4ddaa2](https://github.com/informatievlaanderen/road-registry/commit/b4ddaa24c70b5ce1e379eb84d12098817e6c83cb))

## [3.34.3](https://github.com/informatievlaanderen/road-registry/compare/v3.34.2...v3.34.3) (2023-02-06)


### Bug Fixes

* bump ([#831](https://github.com/informatievlaanderen/road-registry/issues/831)) ([1b7e2e4](https://github.com/informatievlaanderen/road-registry/commit/1b7e2e4c04721e9a10ad22228b9fe81eb4194e03))

## [3.34.2](https://github.com/informatievlaanderen/road-registry/compare/v3.34.1...v3.34.2) (2023-02-06)


### Bug Fixes

* TicketingOptions dependency in RoadSegmentsController ([#826](https://github.com/informatievlaanderen/road-registry/issues/826)) ([c38f2dc](https://github.com/informatievlaanderen/road-registry/commit/c38f2dceb5dda8a951bbbcac6ab65e5fdbfa5d7b))

## [3.34.1](https://github.com/informatievlaanderen/road-registry/compare/v3.34.0...v3.34.1) (2023-02-03)


### Bug Fixes

* change pagesize to 1000 ([#823](https://github.com/informatievlaanderen/road-registry/issues/823)) ([dfbde85](https://github.com/informatievlaanderen/road-registry/commit/dfbde85ef886227a561beeb02fb03592db9cd0cd))
* fix sonar security warning ([bf43f86](https://github.com/informatievlaanderen/road-registry/commit/bf43f8678d47d87da1d14900f13b16515cfff82b))
* publish uploaded message after the SQS message got published ([#824](https://github.com/informatievlaanderen/road-registry/issues/824)) ([817e352](https://github.com/informatievlaanderen/road-registry/commit/817e3524caa5fa48b88a519fe22f78fb79b61115))
* run multiple lambda tests at once ([#825](https://github.com/informatievlaanderen/road-registry/issues/825)) ([bbb44d6](https://github.com/informatievlaanderen/road-registry/commit/bbb44d65ae339e495e3ea156df7a31cea2c75175))

# [3.34.0](https://github.com/informatievlaanderen/road-registry/compare/v3.33.8...v3.34.0) (2023-02-02)


### Features

* WR-508 Add RoadSegmentOutline endpoint ([#821](https://github.com/informatievlaanderen/road-registry/issues/821)) ([762a95b](https://github.com/informatievlaanderen/road-registry/commit/762a95b73f438eebc114c2c3694485d21c69fb6a))

## [3.33.8](https://github.com/informatievlaanderen/road-registry/compare/v3.33.7...v3.33.8) (2023-02-02)


### Bug Fixes

* Add implementation for ILogger which uses LambdaLogger static class ([#819](https://github.com/informatievlaanderen/road-registry/issues/819)) ([ba51136](https://github.com/informatievlaanderen/road-registry/commit/ba51136c108e648a8d25072a59bd42c135db7fda))
* wait for InnerHandleAsync correctly before logging the end ([#820](https://github.com/informatievlaanderen/road-registry/issues/820)) ([7f81e60](https://github.com/informatievlaanderen/road-registry/commit/7f81e601a224a1fde436eaa0cf40e71ae0f87851))

## [3.33.7](https://github.com/informatievlaanderen/road-registry/compare/v3.33.6...v3.33.7) (2023-02-01)


### Bug Fixes

* Add loggers inside Lambda handler and RoadNetworks ([#818](https://github.com/informatievlaanderen/road-registry/issues/818)) ([e63875a](https://github.com/informatievlaanderen/road-registry/commit/e63875a3f82a70941991dfef37f16dbd2133e224))
* Drop wms beginoperator column ([1c340f8](https://github.com/informatievlaanderen/road-registry/commit/1c340f8b8672bcbaaa3e513a5830e84b7ac78f9b))

## [3.33.6](https://github.com/informatievlaanderen/road-registry/compare/v3.33.5...v3.33.6) (2023-01-31)


### Bug Fixes

* generate RoadSegmentNotFoundResponseExamples correctly ([#816](https://github.com/informatievlaanderen/road-registry/issues/816)) ([fe72954](https://github.com/informatievlaanderen/road-registry/commit/fe729541b8b2910b91ecd79eebd58c816cc96c4f))
* load roadsegments in pages ([#815](https://github.com/informatievlaanderen/road-registry/issues/815)) ([1cb9cc3](https://github.com/informatievlaanderen/road-registry/commit/1cb9cc3c078b96beb32aac9b9b941b21ed2084dd))

## [3.33.5](https://github.com/informatievlaanderen/road-registry/compare/v3.33.4...v3.33.5) (2023-01-30)


### Bug Fixes

* use Unknown operator as default ([#814](https://github.com/informatievlaanderen/road-registry/issues/814)) ([0191909](https://github.com/informatievlaanderen/road-registry/commit/01919098dbce39b3c02939761b293ef4ad6db853))

## [3.33.4](https://github.com/informatievlaanderen/road-registry/compare/v3.33.3...v3.33.4) (2023-01-25)


### Bug Fixes

* add streetnameconsumer-projectionhost to pipeline for all envs ([1aec826](https://github.com/informatievlaanderen/road-registry/commit/1aec826037519935d0ff2392ed77b779d11cb7ee))
* WR-579 use translated messages for upload validation ([8cea4e6](https://github.com/informatievlaanderen/road-registry/commit/8cea4e6774673251db35ac512c419fa8bdff70bc))

## [3.33.3](https://github.com/informatievlaanderen/road-registry/compare/v3.33.2...v3.33.3) (2023-01-25)


### Bug Fixes

* add road-registry-backoffice-messaginghost-sqs to PRD deployment ([b2d1f23](https://github.com/informatievlaanderen/road-registry/commit/b2d1f23bb44df22f2ebafc998160c0265db0cdda))
* remove streetnameconsumer-projectionhost from PRD image push ([dee511f](https://github.com/informatievlaanderen/road-registry/commit/dee511fcee57f4a07b7b4abf8073e34244bf5d84))
* remove streetnameconsumer-projectionhost from Push to TST ([b04abac](https://github.com/informatievlaanderen/road-registry/commit/b04abacb4777f2eeaa1577d713d6b9341b28730e))

## [3.33.2](https://github.com/informatievlaanderen/road-registry/compare/v3.33.1...v3.33.2) (2023-01-25)


### Bug Fixes

* kafka producer snapshot projections ([4163111](https://github.com/informatievlaanderen/road-registry/commit/416311170836e51c4d91d1455d34c3bf874c80d2))
* WR-397 add streetnameconsumer-projectionhost to pipelines ([#806](https://github.com/informatievlaanderen/road-registry/issues/806)) ([a058dac](https://github.com/informatievlaanderen/road-registry/commit/a058dac1fafbe829cf3e14192597aa5ce9c8bc84))
* WR-578 remove "gevalideerd" message in activity feed + lambda pipelines + add missing translation for problem `IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction` ([#810](https://github.com/informatievlaanderen/road-registry/issues/810)) ([44f21ea](https://github.com/informatievlaanderen/road-registry/commit/44f21ea4116c7dd7ba1d01232d5360898e5d361c))

## [3.33.1](https://github.com/informatievlaanderen/road-registry/compare/v3.33.0...v3.33.1) (2023-01-24)


### Bug Fixes

* lambda deployment ([#805](https://github.com/informatievlaanderen/road-registry/issues/805)) ([8a9afa4](https://github.com/informatievlaanderen/road-registry/commit/8a9afa45bf0013420d13f471fe4450e778ee0a5e))

# [3.33.0](https://github.com/informatievlaanderen/road-registry/compare/v3.32.4...v3.33.0) (2023-01-24)


### Bug Fixes

* WR-390 increment roadsegment Version/GeometryVersion when the roadsegment changes ([#803](https://github.com/informatievlaanderen/road-registry/issues/803)) ([b50022c](https://github.com/informatievlaanderen/road-registry/commit/b50022c91e77f435f0f59243322430b855b30714))


### Features

* WR-550 show upload received message with before feature-compare immediately after upload  ([#802](https://github.com/informatievlaanderen/road-registry/issues/802)) ([2d4ed27](https://github.com/informatievlaanderen/road-registry/commit/2d4ed279e16505a029be80b3e51dcbc62035b4f1))

## [3.32.4](https://github.com/informatievlaanderen/road-registry/compare/v3.32.3...v3.32.4) (2023-01-18)


### Bug Fixes

* calculate Polyline measure values when missing ([#801](https://github.com/informatievlaanderen/road-registry/issues/801)) ([23b27e2](https://github.com/informatievlaanderen/road-registry/commit/23b27e2931fb4ff8e51f9438a907fad3a9a71d26))

## [3.32.3](https://github.com/informatievlaanderen/road-registry/compare/v3.32.2...v3.32.3) (2023-01-17)


### Bug Fixes

* make ApiKeyAuthAttribute internal ([#799](https://github.com/informatievlaanderen/road-registry/issues/799)) ([918748e](https://github.com/informatievlaanderen/road-registry/commit/918748e299609eb392b2d1e33afbd277f72b1c6a))

## [3.32.2](https://github.com/informatievlaanderen/road-registry/compare/v3.32.1...v3.32.2) (2023-01-17)


### Bug Fixes

* random failures of ArchiveIdTests ([#798](https://github.com/informatievlaanderen/road-registry/issues/798)) ([735b06c](https://github.com/informatievlaanderen/road-registry/commit/735b06c12007380197e776425b696dfa031795aa))

## [3.32.1](https://github.com/informatievlaanderen/road-registry/compare/v3.32.0...v3.32.1) (2023-01-17)


### Bug Fixes

* add extract files to feature compare before validation ([#796](https://github.com/informatievlaanderen/road-registry/issues/796)) ([5ecee48](https://github.com/informatievlaanderen/road-registry/commit/5ecee488031e4bd1e2e0e3fd2195acc3a48d6357))
* add validation for measure values when uploading ([#797](https://github.com/informatievlaanderen/road-registry/issues/797)) ([a1358a0](https://github.com/informatievlaanderen/road-registry/commit/a1358a04f5bc3b501e108e2e216c68e31de032a4))

# [3.32.0](https://github.com/informatievlaanderen/road-registry/compare/v3.31.1...v3.32.0) (2023-01-16)


### Bug Fixes

* Add description from the request into the extract itself ([f36550a](https://github.com/informatievlaanderen/road-registry/commit/f36550a028ed348ddf762ea4966b711c11b1e157))
* add lambda to TST release ([#791](https://github.com/informatievlaanderen/road-registry/issues/791)) ([9b732f9](https://github.com/informatievlaanderen/road-registry/commit/9b732f9f877ceedf3307c415490480b9b99f19f7))
* Forward extract description into Change method on RoadNetwork ([c22e19a](https://github.com/informatievlaanderen/road-registry/commit/c22e19ac059ae45608f647472acfa30e95701286))
* Pass description into last message for RoadNetworkChangeFeedProjection ([ead032c](https://github.com/informatievlaanderen/road-registry/commit/ead032cbbd410449f3d8cf73583661a32cdef1c1))
* separate loading activity top from loading bottom ([443b55d](https://github.com/informatievlaanderen/road-registry/commit/443b55d68bb2caf369380b01b1284d57e16f06ee))
* Use double quotes around description inside changefeed projection ([53a325c](https://github.com/informatievlaanderen/road-registry/commit/53a325c63944f5a27d07bee80c27c1e63d75bc65))
* use RequestId as Description for API extract requests ([612484c](https://github.com/informatievlaanderen/road-registry/commit/612484c686ce1386c5784452d52b45d41dc19829))


### Features

* Auto load new activity ([27c4434](https://github.com/informatievlaanderen/road-registry/commit/27c44343ce5edfd3eb4579d84cda4d9845cfd7de))

## [3.31.1](https://github.com/informatievlaanderen/road-registry/compare/v3.31.0...v3.31.1) (2023-01-11)


### Bug Fixes

* add LastEventHash to custom spatial query ([#786](https://github.com/informatievlaanderen/road-registry/issues/786)) ([2be8bdb](https://github.com/informatievlaanderen/road-registry/commit/2be8bdb6bcf46f545ddf3dd97c6b0ba7466051c9))
* Add logging and fix double registration inside messaging host ([#785](https://github.com/informatievlaanderen/road-registry/issues/785)) ([4953e32](https://github.com/informatievlaanderen/road-registry/commit/4953e32e440377dc647d68aa6aa1124ec3b707a2))
* add producer snapshot to release.yml ([263343f](https://github.com/informatievlaanderen/road-registry/commit/263343fe4f5436e63d2b65ff662c34f573c072a2))

# [3.31.0](https://github.com/informatievlaanderen/road-registry/compare/v3.30.0...v3.31.0) (2023-01-10)


### Features

* use ticketing system for link/unlink streetname ([#776](https://github.com/informatievlaanderen/road-registry/issues/776)) ([cb8c071](https://github.com/informatievlaanderen/road-registry/commit/cb8c071fbcacff2b314d92628feaadd2170a8478))

# [3.30.0](https://github.com/informatievlaanderen/road-registry/compare/v3.29.4...v3.30.0) (2022-12-29)


### Features

* Add info from BESCHRIJV field inside Transactiezones.dbf when performing an upload ([#774](https://github.com/informatievlaanderen/road-registry/issues/774)) ([b24a821](https://github.com/informatievlaanderen/road-registry/commit/b24a821b98caea7de16d66a5585e4a3205f6f407))

## [3.29.4](https://github.com/informatievlaanderen/road-registry/compare/v3.29.3...v3.29.4) (2022-12-27)


### Bug Fixes

* add producer snapshot projectionstates to status page ([#773](https://github.com/informatievlaanderen/road-registry/issues/773)) ([4fb328a](https://github.com/informatievlaanderen/road-registry/commit/4fb328ac7025246a9612c52a0bff0f5844b16338))

## [3.29.3](https://github.com/informatievlaanderen/road-registry/compare/v3.29.2...v3.29.3) (2022-12-26)


### Bug Fixes

* Message consumer for feature compare ([#772](https://github.com/informatievlaanderen/road-registry/issues/772)) ([b4d7774](https://github.com/informatievlaanderen/road-registry/commit/b4d7774ca9dd3cddfe0bd801ab262a84835eb55c))

## [3.29.2](https://github.com/informatievlaanderen/road-registry/compare/v3.29.1...v3.29.2) (2022-12-23)


### Bug Fixes

* Add logging into roadnetwork changes archive handlers ([6e02e76](https://github.com/informatievlaanderen/road-registry/commit/6e02e76736e6741f6c4dceab9cc0e12a95febefd))
* Add missing instantiator parameters ([5410d03](https://github.com/informatievlaanderen/road-registry/commit/5410d03a2a60e6f731a2a8d7125613178c04aedd))
* Changed logger statement into something RR ([024c15c](https://github.com/informatievlaanderen/road-registry/commit/024c15ccc951b3fb80970f86e02e31818e964bda))

## [3.29.1](https://github.com/informatievlaanderen/road-registry/compare/v3.29.0...v3.29.1) (2022-12-22)


### Bug Fixes

* Add support for JObject ([6675483](https://github.com/informatievlaanderen/road-registry/commit/66754833ead3324874ee93c0611f80288d9da931))

# [3.29.0](https://github.com/informatievlaanderen/road-registry/compare/v3.28.2...v3.29.0) (2022-12-22)


### Bug Fixes

* bug wr-270 error message ([a8544de](https://github.com/informatievlaanderen/road-registry/commit/a8544de8032e9aa6faf49a85cfb01d20833a5ff6))
* Bump runtime dependencies version from 7.0.0 to 7.0.1 ([#767](https://github.com/informatievlaanderen/road-registry/issues/767)) ([5505d40](https://github.com/informatievlaanderen/road-registry/commit/5505d4074ab1e3f36bdee0e01dc691c0d57afc05))


### Features

* Update description on the change feed projection ([5cb7c39](https://github.com/informatievlaanderen/road-registry/commit/5cb7c39b992bfff1c6eec2d63e7e1621743a06b7))

## [3.28.2](https://github.com/informatievlaanderen/road-registry/compare/v3.28.1...v3.28.2) (2022-12-20)


### Bug Fixes

* Remove all AWS environment variables from code ([eb2515b](https://github.com/informatievlaanderen/road-registry/commit/eb2515bede139dc981797d9427fdc0dff8ccf9ab))
* UI change header color & title according to env ([cef1345](https://github.com/informatievlaanderen/road-registry/commit/cef134559501f28c320a2f8504a02a045ec51970))

## [3.28.1](https://github.com/informatievlaanderen/road-registry/compare/v3.28.0...v3.28.1) (2022-12-16)


### Bug Fixes

* add prj file to test zip ([c77b86e](https://github.com/informatievlaanderen/road-registry/commit/c77b86e8fdfc1203985d7615f26a235b64c1a0e0))
* add tests for Basic structure upload ([be1a154](https://github.com/informatievlaanderen/road-registry/commit/be1a154051c4ad7ac489bb45d047b9ee030d6b71))
* add unit tests for before featurecompare validator ([e9f7fcc](https://github.com/informatievlaanderen/road-registry/commit/e9f7fcc3758f7f4fdc75c1c86b98a6e5dd0a3e56))
* feaure-compare upload must be able to deal with multiple DBF schemas ([874f5b0](https://github.com/informatievlaanderen/road-registry/commit/874f5b0313b7c04c554f57e92d58ccb7da346b27))
* keep original schema structure for download, seperate from basic schema ([a9800f2](https://github.com/informatievlaanderen/road-registry/commit/a9800f2bb26a48b95f0f500eefc965cb8809bd1a))
* undo commented out code ([1a2b42a](https://github.com/informatievlaanderen/road-registry/commit/1a2b42a72103c98f6c75f0d96f70b2ab12c56af3))

# [3.28.0](https://github.com/informatievlaanderen/road-registry/compare/v3.27.0...v3.28.0) (2022-12-15)


### Features

* add GradeSeparatedJunction snapshot projection ([ad08816](https://github.com/informatievlaanderen/road-registry/commit/ad08816eeb2518f26fd11d2dab362238e6ede525))
* add nationalroad snapshot producer ([#748](https://github.com/informatievlaanderen/road-registry/issues/748)) ([53fcc7b](https://github.com/informatievlaanderen/road-registry/commit/53fcc7b08b4ee8b0d9a5d24443169e361b3366d7))
* add road segment surface snapshot producer ([c9b6923](https://github.com/informatievlaanderen/road-registry/commit/c9b69235e419683cc1428545860685a76a5ad9b1))

# [3.27.0](https://github.com/informatievlaanderen/road-registry/compare/v3.26.5...v3.27.0) (2022-12-12)


### Bug Fixes

* add edit-endpoint request examples + update Swagger decoration ([#747](https://github.com/informatievlaanderen/road-registry/issues/747)) ([e3888a4](https://github.com/informatievlaanderen/road-registry/commit/e3888a4da7a367c02d9263ac1fe66293edd07d89))
* npm version bumping to fix backoffice UI 504 timeout ([#749](https://github.com/informatievlaanderen/road-registry/issues/749)) ([4ff810f](https://github.com/informatievlaanderen/road-registry/commit/4ff810f35617df2efcf42feaf4dc4209255d59f6))


### Features

* Add handler which can allow SQL timeout without stream processing beeing halted ([#746](https://github.com/informatievlaanderen/road-registry/issues/746)) ([7564d50](https://github.com/informatievlaanderen/road-registry/commit/7564d50c64bc034a250722e1ec7015b7075e3024))
* add road segment to snapshot producer ([#741](https://github.com/informatievlaanderen/road-registry/issues/741)) ([b9b946f](https://github.com/informatievlaanderen/road-registry/commit/b9b946f6e5205162113ecbf6cb1ec9f8fac5febc))

## [3.26.5](https://github.com/informatievlaanderen/road-registry/compare/v3.26.4...v3.26.5) (2022-12-08)


### Bug Fixes

* remove feature-compare container start system ([#745](https://github.com/informatievlaanderen/road-registry/issues/745)) ([f9ff724](https://github.com/informatievlaanderen/road-registry/commit/f9ff724660c1d92353f9d71e63f91e8b3ac814a9))

## [3.26.4](https://github.com/informatievlaanderen/road-registry/compare/v3.26.3...v3.26.4) (2022-12-07)


### Bug Fixes

* Straatnaam koppelen docs ([7b988b6](https://github.com/informatievlaanderen/road-registry/commit/7b988b6bc9b2c34bb05d3835b675e51df9db1ee2))

## [3.26.3](https://github.com/informatievlaanderen/road-registry/compare/v3.26.2...v3.26.3) (2022-12-07)


### Bug Fixes

* Add docker build steps to readme for tst/stg/prd environments ([596ce21](https://github.com/informatievlaanderen/road-registry/commit/596ce21baea667cf8f50354aa46061a1cabfd0d6))
* Correct env variable name ([f69625a](https://github.com/informatievlaanderen/road-registry/commit/f69625a65decb861d593393b08510dd524b14a33))
* UI env configurations ([ecb39df](https://github.com/informatievlaanderen/road-registry/commit/ecb39df16216ab3e92f73f1b14dbd14ccfb32492))

## [3.26.2](https://github.com/informatievlaanderen/road-registry/compare/v3.26.1...v3.26.2) (2022-12-07)


### Bug Fixes

* remove messaginghost-sqs from production release ([eebae9b](https://github.com/informatievlaanderen/road-registry/commit/eebae9b6a0f5fa1eb37a7c166b01850c3abfca44))

## [3.26.1](https://github.com/informatievlaanderen/road-registry/compare/v3.26.0...v3.26.1) (2022-12-06)


### Bug Fixes

* Add paket.lock for build ([6e7804e](https://github.com/informatievlaanderen/road-registry/commit/6e7804ea828068d52c53a26517f5664becd85305))
* Append featuretoggles to window instead of overwrite ([bc4c21a](https://github.com/informatievlaanderen/road-registry/commit/bc4c21a6ce29ad33878f9043cf10ec8f327bbd05))
* Paket dependencies and Docker files ([#735](https://github.com/informatievlaanderen/road-registry/issues/735)) ([b37431e](https://github.com/informatievlaanderen/road-registry/commit/b37431ecca2914cc580c9d942cceed05a78ec2ae))
* Set extract description max field length to 254 for random test failure ([8e8b18c](https://github.com/informatievlaanderen/road-registry/commit/8e8b18c2e541df5dc615cd2309430401ec79493a))
* unify product & editor schema's ([bffdb2d](https://github.com/informatievlaanderen/road-registry/commit/bffdb2d9dd0f0e60f564241eaf7ee8b5314a8b16))
* unify schema RoadSegments ([909078d](https://github.com/informatievlaanderen/road-registry/commit/909078dc4505b1c1305f710ec639728ea1a9ff8e))

# [3.26.0](https://github.com/informatievlaanderen/road-registry/compare/v3.25.0...v3.26.0) (2022-12-05)


### Features

* Add endpoints to create/delete organizations ([b43ba21](https://github.com/informatievlaanderen/road-registry/commit/b43ba21094cd75f9ac1e5da30a1361f092c8bee0))

# [3.25.0](https://github.com/informatievlaanderen/road-registry/compare/v3.24.3...v3.25.0) (2022-12-02)


### Bug Fixes

* change dependabot.yml for less noise ([3f4ee4f](https://github.com/informatievlaanderen/road-registry/commit/3f4ee4fc9a89a78b60fa7364ca9760a1c657cb0d))


### Features

* Unlink street name from road segment ([c3844c2](https://github.com/informatievlaanderen/road-registry/commit/c3844c228c29adfc74d72f5ed2c2eba9a44e2d45))

## [3.24.3](https://github.com/informatievlaanderen/road-registry/compare/v3.24.2...v3.24.3) (2022-12-01)


### Bug Fixes

* deploy lambda to production & deploy producer-snapshot-projectionhost ([#727](https://github.com/informatievlaanderen/road-registry/issues/727)) ([c934663](https://github.com/informatievlaanderen/road-registry/commit/c934663ab608e2a4dfaca77957c63af7055b0667))

## [3.24.2](https://github.com/informatievlaanderen/road-registry/compare/v3.24.1...v3.24.2) (2022-12-01)


### Bug Fixes

* exclude messaginghost-kafka ([3d55596](https://github.com/informatievlaanderen/road-registry/commit/3d55596d87c8c562a5e611eaa3c4df5c06743635))

## [3.24.1](https://github.com/informatievlaanderen/road-registry/compare/v3.24.0...v3.24.1) (2022-11-30)


### Bug Fixes

* correct typo ([5946ae5](https://github.com/informatievlaanderen/road-registry/commit/5946ae52703b9b005f6aa3dae6f3a1900651a4b8))
* remove producer-snapshot-projectionhost ([#724](https://github.com/informatievlaanderen/road-registry/issues/724)) ([43df566](https://github.com/informatievlaanderen/road-registry/commit/43df5664c064d576d58cc3a17cff0bf2d9bb8027))

# [3.24.0](https://github.com/informatievlaanderen/road-registry/compare/v3.23.6...v3.24.0) (2022-11-30)


### Bug Fixes

* correct typo in release.yml ([e796925](https://github.com/informatievlaanderen/road-registry/commit/e796925509b038411d84282d8ad9bbb19554ac75))


### Features

* Street name validation for linking to road segment ([a24f5bf](https://github.com/informatievlaanderen/road-registry/commit/a24f5bf6e9a54e77eb99bc7708f59b0ad8ed3da6))

## [3.23.6](https://github.com/informatievlaanderen/road-registry/compare/v3.23.5...v3.23.6) (2022-11-30)


### Bug Fixes

* remove lambda from production again ([89b4043](https://github.com/informatievlaanderen/road-registry/commit/89b40434e21c72ac834248938ea95efa045fe4d6))

## [3.23.5](https://github.com/informatievlaanderen/road-registry/compare/v3.23.4...v3.23.5) (2022-11-30)


### Bug Fixes

* exclude lambda from production ([#717](https://github.com/informatievlaanderen/road-registry/issues/717)) ([808a951](https://github.com/informatievlaanderen/road-registry/commit/808a9511fa6980decd35de3b70a49423a53b9b7c))

## [3.23.4](https://github.com/informatievlaanderen/road-registry/compare/v3.23.3...v3.23.4) (2022-11-30)


### Bug Fixes

* correct release.yml ([2f73274](https://github.com/informatievlaanderen/road-registry/commit/2f7327499650c783ffb6ec91d2b2b6ee359c1347))
* fix flaky test ([886c698](https://github.com/informatievlaanderen/road-registry/commit/886c698342eb4a9f9766ba719cef5e40321e38eb))
* include production in release.yml ([#715](https://github.com/informatievlaanderen/road-registry/issues/715)) ([9730e6e](https://github.com/informatievlaanderen/road-registry/commit/9730e6e1eb2a00948546fb5ca93c42ad924f8c89))
* really fix flaky test ([b26ae6c](https://github.com/informatievlaanderen/road-registry/commit/b26ae6c13f1c71c2ef48f0d9b9a7cb5f6d7dfb3b))
* use new release.yml ([ec417b6](https://github.com/informatievlaanderen/road-registry/commit/ec417b6a19285950e9dc76e19a45df9c1df02c66))

## [3.23.3](https://github.com/informatievlaanderen/road-registry/compare/v3.23.2...v3.23.3) (2022-11-29)


### Bug Fixes

* fix workflow again again ([9a15235](https://github.com/informatievlaanderen/road-registry/commit/9a15235a7fde21746f808dc651bf4f2e32614a07))

## [3.23.2](https://github.com/informatievlaanderen/road-registry/compare/v3.23.1...v3.23.2) (2022-11-29)


### Bug Fixes

* fix workflow again ([#711](https://github.com/informatievlaanderen/road-registry/issues/711)) ([afd6e07](https://github.com/informatievlaanderen/road-registry/commit/afd6e07e02b46d07e71ae8a8ca788da5b159a3ae))

## [3.23.1](https://github.com/informatievlaanderen/road-registry/compare/v3.23.0...v3.23.1) (2022-11-29)


### Bug Fixes

* fix workflow & add producer-snapshot-projectionhost ([22ff556](https://github.com/informatievlaanderen/road-registry/commit/22ff5563e901d21c17cd45ffa0e21a8656bed25b))
* remove empty statement ([9a176b6](https://github.com/informatievlaanderen/road-registry/commit/9a176b62bff034f357558a2e03c145febff42c55))

# [3.23.0](https://github.com/informatievlaanderen/road-registry/compare/v3.22.0...v3.23.0) (2022-11-29)


### Bug Fixes

* correct workflow ([ab03af1](https://github.com/informatievlaanderen/road-registry/commit/ab03af18bfaf0e1a2eafede12b992d7e739e86b1))


### Features

* add kafka produce for roadnode snapshot producer ([a33a0cc](https://github.com/informatievlaanderen/road-registry/commit/a33a0cc103e30dec850392293a5e16f75b71926c))
* add roadnode projections for producer snapshot ([8d2c726](https://github.com/informatievlaanderen/road-registry/commit/8d2c726860bf2880d0881198cca53b2e95b90b60))

# [3.22.0](https://github.com/informatievlaanderen/road-registry/compare/v3.21.4...v3.22.0) (2022-11-29)


### Bug Fixes

* bump dockerimages to dotnet 6 ([30dd236](https://github.com/informatievlaanderen/road-registry/commit/30dd236a35929cb01ec7f7a353b2d016b0b7db2d))
* Check Local in dbcontext for tests ([2461175](https://github.com/informatievlaanderen/road-registry/commit/2461175a4652325e24e0dbd50fb12bce1fcd61df))
* correct streetnameconsumer csproj ([556f519](https://github.com/informatievlaanderen/road-registry/commit/556f519b87d3fce84de16c1dbb6d9949422e1a3e))
* Get validator through DI ([4510cd1](https://github.com/informatievlaanderen/road-registry/commit/4510cd18ac9fe412ebf12ea09d1766db12facca4))
* Guarantee default organization name to be different than actual organization name for test ([f6442cb](https://github.com/informatievlaanderen/road-registry/commit/f6442cb451d9ccef0484e72e723951542ad2c802))
* Let validator do null check ([69bb272](https://github.com/informatievlaanderen/road-registry/commit/69bb2729a6baf8d7d5a438d7a0512e471f46870a))
* Tests ([d4f3ee3](https://github.com/informatievlaanderen/road-registry/commit/d4f3ee33ddd8a36a8d7d0f4996fb5e63fd837972))
* update workflows ([8207ed3](https://github.com/informatievlaanderen/road-registry/commit/8207ed3ba053dead511f010cc203b63fd284c8cb))


### Features

* Add European/National road segments to WMS ([#697](https://github.com/informatievlaanderen/road-registry/issues/697)) ([4daa345](https://github.com/informatievlaanderen/road-registry/commit/4daa345bd9ea4bad0725375cd65d355d8504bb14))
* Add RenameOrganization message ([6ae36cf](https://github.com/informatievlaanderen/road-registry/commit/6ae36cfe21557c5268be822bee9401ac0cda00d5))
* Link RoadSegment to StreetName ([f129720](https://github.com/informatievlaanderen/road-registry/commit/f1297205f7038af355e6e89490e88ac6bd2d1e37))
* Rename an organization ([9c8cfea](https://github.com/informatievlaanderen/road-registry/commit/9c8cfeac57817e7906852fc529660f9291c6e19b))

## [3.21.4](https://github.com/informatievlaanderen/road-registry/compare/v3.21.3...v3.21.4) (2022-11-17)


### Bug Fixes

* Docker file ([6c6a59a](https://github.com/informatievlaanderen/road-registry/commit/6c6a59a72c08de1165422035ed06717cf610e920))
* Docker installer issue ([118869a](https://github.com/informatievlaanderen/road-registry/commit/118869ac55b638cd77ebbb1fc14842ec3390e870))


### Reverts

* Revert "Revert "feat: Add docker into messaginghost after SQS tweaks"" ([9b7f1eb](https://github.com/informatievlaanderen/road-registry/commit/9b7f1ebf88428d9ab9b5e24524724be55d05f31d))

## [3.21.3](https://github.com/informatievlaanderen/road-registry/compare/v3.21.2...v3.21.3) (2022-11-15)


### Bug Fixes

* Include documentation files into product download ([#693](https://github.com/informatievlaanderen/road-registry/issues/693)) ([c54844f](https://github.com/informatievlaanderen/road-registry/commit/c54844f4598782f25d9421b769fcc5910e4eea25))
* Resources to use in product download should give error when missing ([b4e2bb6](https://github.com/informatievlaanderen/road-registry/commit/b4e2bb60a2c3ec68175509909c3757071d66d945))

## [3.21.2](https://github.com/informatievlaanderen/road-registry/compare/v3.21.1...v3.21.2) (2022-11-15)


### Bug Fixes

* Use Lambert SRID when parsing WKT ([70f08b1](https://github.com/informatievlaanderen/road-registry/commit/70f08b10af93a74e4fb4a6699b28630f1b528482))

## [3.21.1](https://github.com/informatievlaanderen/road-registry/compare/v3.21.0...v3.21.1) (2022-11-14)


### Bug Fixes

* ApiKeyAuth visibility with incorrect incoming api token ([b84575c](https://github.com/informatievlaanderen/road-registry/commit/b84575c38b1e08d296af0d9bf8ecf31377a2bfb6))
* Development settings ([9fa7f08](https://github.com/informatievlaanderen/road-registry/commit/9fa7f089f7b0e17b408085534a496dd8398ede46))
* Incorrect header translation ([ee54c34](https://github.com/informatievlaanderen/road-registry/commit/ee54c34d8ec913a7e7249f0fae6a25af64406b86))

# [3.21.0](https://github.com/informatievlaanderen/road-registry/compare/v3.20.2...v3.21.0) (2022-11-14)


### Bug Fixes

* Add apikey check to downloads ([094f0d6](https://github.com/informatievlaanderen/road-registry/commit/094f0d6dc2b7c7aef98e931af2975794d6c1f8f1))
* Add feature compare feature toggle for the UI ([5c0e31e](https://github.com/informatievlaanderen/road-registry/commit/5c0e31e44b0964fdd0b0aaaabb128527a315ccdb))
* Misspelled DynamoDbClientOptions ([20dec5d](https://github.com/informatievlaanderen/road-registry/commit/20dec5d627bb7aad6424bd2e3f48cb6d5b058fd5))
* Show loader when downloading ([b296668](https://github.com/informatievlaanderen/road-registry/commit/b296668de3f39e090b2d984b9213fa805aaeed3c))
* SonarCloud fix on seperated if statement ([dda2e59](https://github.com/informatievlaanderen/road-registry/commit/dda2e59b096801c2b221afc217d7f36c451c8846))


### Features

* Add streetname kafka consumer ([#672](https://github.com/informatievlaanderen/road-registry/issues/672)) ([59392bc](https://github.com/informatievlaanderen/road-registry/commit/59392bc723115d8ff6f31f2d7f9b7796eafdc016))
* Converted into multi role dynamo db support ([8a1f1f2](https://github.com/informatievlaanderen/road-registry/commit/8a1f1f21e89eaafdcbbacaf85f37121be52db50c))
* DynamoDB support for API key authentication ([041cf9c](https://github.com/informatievlaanderen/road-registry/commit/041cf9c91a5025a8d4c59de6bbdb946d83ebc2be))
* SonarCube warning cleanup ([5ab760f](https://github.com/informatievlaanderen/road-registry/commit/5ab760f8ab3855f3e3d6e8f8e733a99eebb33eff))

## [3.20.2](https://github.com/informatievlaanderen/road-registry/compare/v3.20.1...v3.20.2) (2022-11-08)


### Bug Fixes

* Use SQS queue url instead of name when publishing ([09b08da](https://github.com/informatievlaanderen/road-registry/commit/09b08da9d03f970030d6cdb3efa89f81a0ae7a1b))

## [3.20.1](https://github.com/informatievlaanderen/road-registry/compare/v3.20.0...v3.20.1) (2022-11-08)


### Bug Fixes

* Use FluentValidation ValidateAsync ([19192e0](https://github.com/informatievlaanderen/road-registry/commit/19192e0e56c6f4ffb8f3523b34814d3d3ab15746))

# [3.20.0](https://github.com/informatievlaanderen/road-registry/compare/v3.19.3...v3.20.0) (2022-11-08)


### Bug Fixes

* Pass correct value to ArgumentNullException.ThrowIfNull ([#669](https://github.com/informatievlaanderen/road-registry/issues/669)) ([17868d7](https://github.com/informatievlaanderen/road-registry/commit/17868d7f6d2ab7a18455e3e69adaeac4ad91011e))
* update testje.yml ([a0d1c43](https://github.com/informatievlaanderen/road-registry/commit/a0d1c437420018aa1fe1d919502e6c03ac0864cc))
* update testje.yml ([07f6f93](https://github.com/informatievlaanderen/road-registry/commit/07f6f93b29742e686501240a8bde0e85ec034af5))


### Features

* Feature compare ([#659](https://github.com/informatievlaanderen/road-registry/issues/659)) ([4f506b5](https://github.com/informatievlaanderen/road-registry/commit/4f506b5ff9aa5285e8bbac7f780869feb2e6a304))

## [3.19.3](https://github.com/informatievlaanderen/road-registry/compare/v3.19.2...v3.19.3) (2022-11-07)


### Bug Fixes

* add braces ([f819fa4](https://github.com/informatievlaanderen/road-registry/commit/f819fa4f1ea945ba72b1c910feba1328a21af38a))
* add protected ctor ([50f1dfc](https://github.com/informatievlaanderen/road-registry/commit/50f1dfc7b2e1291a0935a89ae9082210d9cd1655))
* conform to serializable pattern ([6555e5b](https://github.com/informatievlaanderen/road-registry/commit/6555e5bf57ee9b944e7421092fba474ff3087505))
* don't throw general exceptions ([9e34485](https://github.com/informatievlaanderen/road-registry/commit/9e344850544281071956770d81beec38c164d8dc))
* fill empty blocks of code ([99f62df](https://github.com/informatievlaanderen/road-registry/commit/99f62df94f1f33af3538a492e65524ca9d3bd57b))
* Remove FeatureCompare lambda's from Push to Production ([#666](https://github.com/informatievlaanderen/road-registry/issues/666)) ([1be23dc](https://github.com/informatievlaanderen/road-registry/commit/1be23dc4a2110e23eeebc4335361e27c8b746f58))
* remove unneeded parameters ([bcbb1b1](https://github.com/informatievlaanderen/road-registry/commit/bcbb1b1ed1920d6b804c915a0c7dea90fce861f1))
* remove unused fields ([9f30970](https://github.com/informatievlaanderen/road-registry/commit/9f30970571e1c62eb60f1b436af56221d9f8d304))
* remove unused variables ([6b32b5a](https://github.com/informatievlaanderen/road-registry/commit/6b32b5a4be8221127c6c455c1b8530b381b8f0de))
* rename parameters ([89998eb](https://github.com/informatievlaanderen/road-registry/commit/89998ebcd58706f20c506bb77315def3b6f6f95a))
* throw exception with correct message ([30c5ec3](https://github.com/informatievlaanderen/road-registry/commit/30c5ec3ffe2408bdea4012469d4fc573cc192347))
* utility classes static or protected ctor ([817ab25](https://github.com/informatievlaanderen/road-registry/commit/817ab25dc62672a2dd05fbaaa3a5ac739f8b7cbe))

## [3.19.2](https://github.com/informatievlaanderen/road-registry/compare/v3.19.1...v3.19.2) (2022-11-04)


### Bug Fixes

* Add Dutch validation messages for download extract ([7cc292f](https://github.com/informatievlaanderen/road-registry/commit/7cc292fb7580191e587af7b1fa7b00891647e5eb))
* add nuget to dependabot ([5e86cc8](https://github.com/informatievlaanderen/road-registry/commit/5e86cc836b0034af88ab795aad266d1131fc58dd))
* build on pull requests ([ee00146](https://github.com/informatievlaanderen/road-registry/commit/ee00146b7eb4c4c4ef4ee9c5e1a4e729e64bd9c2))
* remove main.yml ([1b47af6](https://github.com/informatievlaanderen/road-registry/commit/1b47af6e8d4b0c1b53dff94cf1f6d41480806a57))
* use VBR_SONAR_TOKEN ([a946282](https://github.com/informatievlaanderen/road-registry/commit/a946282e4bd153e61073bf841973497a37aa785a))

## [3.19.1](https://github.com/informatievlaanderen/road-registry/compare/v3.19.0...v3.19.1) (2022-11-03)


### Bug Fixes

* Allow anonymous edge case ([005a5ec](https://github.com/informatievlaanderen/road-registry/commit/005a5ec3e1bf8c9fbec70528a8093b4aa777fe0c))

# [3.19.0](https://github.com/informatievlaanderen/road-registry/compare/v3.18.1...v3.19.0) (2022-11-03)


### Features

* API key through AuthorizationFilterContext instead of ActionExecutingContext ([604f01c](https://github.com/informatievlaanderen/road-registry/commit/604f01c8b9a5c0fb2c480b6b259abb0367e980d2))

## [3.18.1](https://github.com/informatievlaanderen/road-registry/compare/v3.18.0...v3.18.1) (2022-11-03)


### Bug Fixes

* version bump ([906f39d](https://github.com/informatievlaanderen/road-registry/commit/906f39d79225c77f56a7cfcb0f0b06e1261a1523))
* version bump ([d1a2f97](https://github.com/informatievlaanderen/road-registry/commit/d1a2f971923bf4e3e3acf6a5364fa12dce9efa44))

# [3.18.0](https://github.com/informatievlaanderen/road-registry/compare/v3.17.1...v3.18.0) (2022-11-02)


### Bug Fixes

* Change errormessage on upload control ([f6a66d0](https://github.com/informatievlaanderen/road-registry/commit/f6a66d0e8540c768867441bfac62748ce36a8b45))
* register validators in backoffice API ([0938dc6](https://github.com/informatievlaanderen/road-registry/commit/0938dc605f6103593603a960dce0d52950c0e8f2))
* SQS removed from production workflow ([1a76dd7](https://github.com/informatievlaanderen/road-registry/commit/1a76dd78134e22a2102966ac18db203b32e47ddc))
* SQS removed from production workflow ([a43b98d](https://github.com/informatievlaanderen/road-registry/commit/a43b98d61e62041143dcbac7dc127afec8faa5f9))
* use WKT to transfer geometry data to avoid extra internal translations ([c314d79](https://github.com/informatievlaanderen/road-registry/commit/c314d7951857059848d616743fe760fb4cfacf7f))


### Features

* Initialize Vue after authentication check ([8eb9fd5](https://github.com/informatievlaanderen/road-registry/commit/8eb9fd53d90dc7400de087b419f091101f2173bf))
* Initialize Vue after authentication check ([f7495fc](https://github.com/informatievlaanderen/road-registry/commit/f7495fc985e3913287f712a8d8e16affd000e55d))
* Remove buffer from contour workflow ([9f5659c](https://github.com/informatievlaanderen/road-registry/commit/9f5659c2acb58687d57f4a0d0247ec870bde9d5a))
* Skip buffering when buffer size equals zero ([d9ffa6c](https://github.com/informatievlaanderen/road-registry/commit/d9ffa6cffb3a213593003d4795e816006f446669))

## [3.17.1](https://github.com/informatievlaanderen/road-registry/compare/v3.17.0...v3.17.1) (2022-11-02)


### Bug Fixes

* remove kafka from all pipelines ([35a30c0](https://github.com/informatievlaanderen/road-registry/commit/35a30c09f67015fb88eed893491cda2a810fcba8))
* remove kafka from release pipeline ([5dd5add](https://github.com/informatievlaanderen/road-registry/commit/5dd5add8d771634ef3313bfc3c51b0e7f0105c52))

# [3.17.0](https://github.com/informatievlaanderen/road-registry/compare/v3.16.1...v3.17.0) (2022-11-02)


### Bug Fixes

* missing solution configurations ([0bc6159](https://github.com/informatievlaanderen/road-registry/commit/0bc6159bac9c225cbe14bed77d1f963cd61cf807))
* move extensions to main type namespace ([6a75052](https://github.com/informatievlaanderen/road-registry/commit/6a75052bde47335c7d63560ad63b83ea1b97b8f6))
* move kafka logic to separate Handlers library ([40e50d7](https://github.com/informatievlaanderen/road-registry/commit/40e50d755c61efd7c2e450318fb729d812cde139))
* refactor so that IServiceCollection is not needed during ConfigureContainer ([9179528](https://github.com/informatievlaanderen/road-registry/commit/91795284d90ed08829ff636469a532379ed0844f))
* remove kafka application from pipelines ([742c843](https://github.com/informatievlaanderen/road-registry/commit/742c8439149b3cc6c4512d4ed3db334dcd15ba04))
* temporarily disable logic of kafka message processing ([8142a1a](https://github.com/informatievlaanderen/road-registry/commit/8142a1a93386a3ffc284d9ba32347bd9d16e72cc))


### Features

* Add Kafka and Kafka test projects ([d18014d](https://github.com/informatievlaanderen/road-registry/commit/d18014ded6b919cf88da0797cb08bb5645c2d788))
* Add Kafka consumer and pipeline update ([397e8b0](https://github.com/informatievlaanderen/road-registry/commit/397e8b0b8652da8d984943abe465e454138ea32e))
* Add Kafka consumer project shell ([d45c79d](https://github.com/informatievlaanderen/road-registry/commit/d45c79d0be171fd7284659d0424c5eb7783cd36d))
* add MessagingHost.Kafka ([eaebd1b](https://github.com/informatievlaanderen/road-registry/commit/eaebd1bea468329780a8fbfb129057fdc0ddbcf5))
* disable StreetNameConsumer ([fe2794d](https://github.com/informatievlaanderen/road-registry/commit/fe2794dfb69dc0ba61b3afe133a3324f376a5bda))
* Kafka consumer adoption ([777db74](https://github.com/informatievlaanderen/road-registry/commit/777db744a608efb4976ea1fef7034f1085df1bd5))
* move non-host logic to Handlers.Kafka ([1611887](https://github.com/informatievlaanderen/road-registry/commit/1611887079b3ecf86cad129725bfcc293033b98d))
* Program and host builder optimization ([f5ecf96](https://github.com/informatievlaanderen/road-registry/commit/f5ecf96244cc2ed34adb66e0e16b1354bd2c3a90))
* set up kafka streetname consumer ([a4c735e](https://github.com/informatievlaanderen/road-registry/commit/a4c735e11fc42e0124557d7ba23f9037272ba344))
* update paket dependencies to latest ([68de80d](https://github.com/informatievlaanderen/road-registry/commit/68de80d031a7362ce7852ffc8feba237150803fa))
* Updated paket references ([1c2dfdd](https://github.com/informatievlaanderen/road-registry/commit/1c2dfdda86ba82ab0dcc89c7c2dc84b2c10a42e9))

## [3.16.1](https://github.com/informatievlaanderen/road-registry/compare/v3.16.0...v3.16.1) (2022-10-28)


### Bug Fixes

* error message with failed login attempt ([43b8a35](https://github.com/informatievlaanderen/road-registry/commit/43b8a35ea21f5b5fe95b57e2bf73c723cacfd592))
* remove temporary request logging ([8b8974c](https://github.com/informatievlaanderen/road-registry/commit/8b8974cd5431a7840dd2f239e6ee096d75dfa63a))
* remove x-api-key header requirement when x-api-token is being used ([e4e39c2](https://github.com/informatievlaanderen/road-registry/commit/e4e39c21a25a298164c23e15f3187d8abba32bf1))

# [3.16.0](https://github.com/informatievlaanderen/road-registry/compare/v3.15.1...v3.16.0) (2022-10-27)


### Bug Fixes

* Add missing registrations in test startup class ([038c852](https://github.com/informatievlaanderen/road-registry/commit/038c8525e8f20b2e13e614b005ceb2c041498a3d))
* add support for public-api debugging ([df09172](https://github.com/informatievlaanderen/road-registry/commit/df091722461f3c907e68208660279d86750b036c))
* UI router auth check; display validation errors on upload immediately ([32bf448](https://github.com/informatievlaanderen/road-registry/commit/32bf4489279e8eb311f45de387012fee869ac355))


### Features

* Merge from main ([0728dda](https://github.com/informatievlaanderen/road-registry/commit/0728ddac4e95c262486d2fc6d6e1cfd02779d0fb))
* Paket version update ([158c878](https://github.com/informatievlaanderen/road-registry/commit/158c878cdf3f5e5dee73fde4aedf94aa634937dd))
* Safeguard input zip archive validation through feature toggle ([e844f2c](https://github.com/informatievlaanderen/road-registry/commit/e844f2ccec7b96d5cc7e0de9f889d7c53a9869c9))
* Updated FluentValidation and removed deprecated test method ([32c2d8a](https://github.com/informatievlaanderen/road-registry/commit/32c2d8ab3bc298cef7f2d35ae817da3a72f88d9a))

## [3.15.1](https://github.com/informatievlaanderen/road-registry/compare/v3.15.0...v3.15.1) (2022-10-26)


### Bug Fixes

* add current request info logging ([265c109](https://github.com/informatievlaanderen/road-registry/commit/265c109d42ca56081e86698685df3607229d0e41))

# [3.15.0](https://github.com/informatievlaanderen/road-registry/compare/v3.14.0...v3.15.0) (2022-10-26)


### Features

* Fix inconsistent code style and document layout ([a02bc47](https://github.com/informatievlaanderen/road-registry/commit/a02bc471f93662eb06d88d9dce7d0c5eec56a99f))

# [3.14.0](https://github.com/informatievlaanderen/road-registry/compare/v3.13.3...v3.14.0) (2022-10-25)


### Bug Fixes

* add missing [FromBody] attribute for validate-wkt ([c93f11c](https://github.com/informatievlaanderen/road-registry/commit/c93f11c8457bd84cad189d0217b3670427d040fb))
* auth check on routes ([6181320](https://github.com/informatievlaanderen/road-registry/commit/6181320a34209ecba0800748f73ccfa06fa5a31f))
* show validation msg on top of page for download extract ([fd3f56b](https://github.com/informatievlaanderen/road-registry/commit/fd3f56bddfc4621e219e22d055a22c4491f70398))
* use FormData to upload multiple files ([155e6d5](https://github.com/informatievlaanderen/road-registry/commit/155e6d535aab171c0eb36ede8b9c115e980ee313))


### Features

* add file selection to `Download Extract` page ([b1ca0bd](https://github.com/informatievlaanderen/road-registry/commit/b1ca0bd7042e6dbfcb6275889f40cb8048f82145))
* Add handlers and request objects for file collections ([bbf7875](https://github.com/informatievlaanderen/road-registry/commit/bbf7875b85fd2c11f1459f6b0ba1565f6064ad47))
* Add validation for WKT ([bb7e389](https://github.com/informatievlaanderen/road-registry/commit/bb7e389960fceff8df014ab2ec862aaded5e62ea))
* convert uploaded geometry to multipolygon ([09f14ca](https://github.com/informatievlaanderen/road-registry/commit/09f14cae8d322481925efceb54bfd6c345c46894))
* disable submit button when api call is busy ([a4e10c3](https://github.com/informatievlaanderen/road-registry/commit/a4e10c31a251fc9788fc82a62a2887aa4ea1e405))
* finalize tests for download extract by file ([1cd244b](https://github.com/informatievlaanderen/road-registry/commit/1cd244b35cab3997d27ac04a845e07220fc1940d))
* Geometry translator ([4e2ff68](https://github.com/informatievlaanderen/road-registry/commit/4e2ff6843118d989c57680a72bce2c4cf8cfbba0))
* handle validation errors for download extract; remove .shx file ([e619339](https://github.com/informatievlaanderen/road-registry/commit/e619339bde3892f1db6ea52a3c8ea9ee937ea9dc))
* Shape file handler asynchronicity unnecessary ([c8783a9](https://github.com/informatievlaanderen/road-registry/commit/c8783a9bd0c798e39ddeea8faabb21f5b54fb6ef))
* Update handler for shape files ([ae47f4d](https://github.com/informatievlaanderen/road-registry/commit/ae47f4dad3a45b7306d57264bc16df2742e1d52f))
* WKT validation ([f9a736d](https://github.com/informatievlaanderen/road-registry/commit/f9a736ddc479e3a6a00cc2c669827d0d1b36b68b))

## [3.13.3](https://github.com/informatievlaanderen/road-registry/compare/v3.13.2...v3.13.3) (2022-10-21)


### Bug Fixes

* remove api_key from env; remove .map files on build ([4bc218c](https://github.com/informatievlaanderen/road-registry/commit/4bc218cb92f32bb535e19f57b9c2addaca34dbdb))

## [3.13.2](https://github.com/informatievlaanderen/road-registry/compare/v3.13.1...v3.13.2) (2022-10-20)


### Bug Fixes

* add missing IBlobClient registration when using S3BlobClient ([7bdf81b](https://github.com/informatievlaanderen/road-registry/commit/7bdf81b559a8f1bc3ea5f5762da85f2557c08484))

## [3.13.1](https://github.com/informatievlaanderen/road-registry/compare/v3.13.0...v3.13.1) (2022-10-19)


### Bug Fixes

* add testje.yml ([0bdb7f7](https://github.com/informatievlaanderen/road-registry/commit/0bdb7f74ef47450842ab6a5433f6e4428267cea1))
* csproj file cleanup ([b7256c7](https://github.com/informatievlaanderen/road-registry/commit/b7256c7a8db8b04c60933dc62de2ea09168e8976))
* Incorrect blob client ([d558b85](https://github.com/informatievlaanderen/road-registry/commit/d558b85de506454b6e39135da7d00174ac604ae4))
* services indentation ([4a9a6cc](https://github.com/informatievlaanderen/road-registry/commit/4a9a6ccf977c5e21d7f2637cca17be569d64d68b))
* testje workflow name ([2b79bbf](https://github.com/informatievlaanderen/road-registry/commit/2b79bbfda1c4da8e93a7af39bf3a3db7facd5a7f))

# [3.13.0](https://github.com/informatievlaanderen/road-registry/compare/v3.12.1...v3.13.0) (2022-10-19)


### Bug Fixes

* assert correct response for test; remove gitignored xml files ([7b543b7](https://github.com/informatievlaanderen/road-registry/commit/7b543b71bea02ca850bd0732cf04453a5d0b3446))
* Build warning cleanup; File layout refactoring ([7ba86e3](https://github.com/informatievlaanderen/road-registry/commit/7ba86e34fff67d43eac41bfa7ad7d1b9ff1b07b4))
* conform to Serializable pattern ([883587b](https://github.com/informatievlaanderen/road-registry/commit/883587bb16e288e62416b80c7c73e0ece78272f4))
* fix corrupt previous merge ([13dd7fb](https://github.com/informatievlaanderen/road-registry/commit/13dd7fbdd3f2a9f7f72b7ee4384114d455a70285))
* return Retry-After header also when successful ([f94cab2](https://github.com/informatievlaanderen/road-registry/commit/f94cab2996c0323e5dd8bda4ff37763dc4eb5d40))
* Upload test file incorrect merge ([1e64778](https://github.com/informatievlaanderen/road-registry/commit/1e64778eb2ebce377b93f7c1f71c8fb96e0affad))
* use existing response classes with ID properties as strings ([af399d3](https://github.com/informatievlaanderen/road-registry/commit/af399d34307f61950ce4c3f88fdca317274a9674))


### Features

* Add api key authentication through attributes ([ea14b10](https://github.com/informatievlaanderen/road-registry/commit/ea14b10c5ee64811bce0ea65c41c54f44332fb46))
* Remove user secrets id ([bd739d4](https://github.com/informatievlaanderen/road-registry/commit/bd739d44176a97704cf12c32515c664674ef8c2c))

## [3.12.1](https://github.com/informatievlaanderen/road-registry/compare/v3.12.0...v3.12.1) (2022-10-17)


### Bug Fixes

* new release ([103f408](https://github.com/informatievlaanderen/road-registry/commit/103f408deb67252fec56802b7595bbba03e93dc9))

# [3.12.0](https://github.com/informatievlaanderen/road-registry/compare/v3.11.0...v3.12.0) (2022-10-17)


### Bug Fixes

* include solution name in build.yml ([9470f0b](https://github.com/informatievlaanderen/road-registry/commit/9470f0b8a0766cb67e22bbb54d3d37d43a1ff3c5))
* make DownloadEditorNotFoundException conform to serialiizable pattern ([6bb01b0](https://github.com/informatievlaanderen/road-registry/commit/6bb01b0b29ba3a13c51102b7a82ff0e755c34919))
* remove ConvexPolygon for finding integrationdata for extract, use buffer on each underlying polygon of the contour ([dcb1f8d](https://github.com/informatievlaanderen/road-registry/commit/dcb1f8d435109191c35dcc2bb84280613c8b87b8))
* skip test ([d534fb4](https://github.com/informatievlaanderen/road-registry/commit/d534fb42b0dd0abb795f77779c1a2f83639619ed))
* skip test for live debugging ([4fa8457](https://github.com/informatievlaanderen/road-registry/commit/4fa84576e1456c96eb55af60df7eb3770162cf71))
* skip test for live debugging purposes ([78676a8](https://github.com/informatievlaanderen/road-registry/commit/78676a8e08c903639335e7aabf4a44a4de67fb3d))


### Features

* add featuretoggle for FeatureCompare upload endpoint ([6ca59ae](https://github.com/informatievlaanderen/road-registry/commit/6ca59ae8071fa854ffe4ddac43be51ef860ea2b2))
* Performance improvement for query ([4c90b0e](https://github.com/informatievlaanderen/road-registry/commit/4c90b0ef546e412b9c1a297168ccb32470bee034))
* UI terug naar originele post-FC endpoint laten kijken + removal VUE_APP_API_KEY from env files ([61412f9](https://github.com/informatievlaanderen/road-registry/commit/61412f95b35e8d671e82974ba177c49aab482df5))

# [3.11.0](https://github.com/informatievlaanderen/road-registry/compare/v3.10.2...v3.11.0) (2022-10-07)


### Features

* fix tests for restored upload fc endpoint ([4c86c50](https://github.com/informatievlaanderen/road-registry/commit/4c86c507c1df0b6d7838aab4f1a8fd717d84229b))
* fix upload after fc endpoint to accept zip files ([f522712](https://github.com/informatievlaanderen/road-registry/commit/f5227120327af28b2dcc0f1caf179d7ab89d0227))
* restore endpoint upload zip after featurecompare ([5098896](https://github.com/informatievlaanderen/road-registry/commit/509889699b05b4031bcd0e35d13bce142228f5b0))

## [3.10.2](https://github.com/informatievlaanderen/road-registry/compare/v3.10.1...v3.10.2) (2022-10-04)


### Bug Fixes

* wfs migration. make RoadSegment & RodeNode primary key clustered for spatial indexes ([03ac4db](https://github.com/informatievlaanderen/road-registry/commit/03ac4db9d3471e8f19bd54ee4030cc3f5260758c))

## [3.10.1](https://github.com/informatievlaanderen/road-registry/compare/v3.10.0...v3.10.1) (2022-10-04)


### Bug Fixes

* wfs comment GradeSeparatedJunctions, add spatial index, add point geometry to RoadNode ([88c071c](https://github.com/informatievlaanderen/road-registry/commit/88c071c53567ca2744dcfa8132397cfceb9d13a9))

# [3.10.0](https://github.com/informatievlaanderen/road-registry/compare/v3.9.1...v3.10.0) (2022-10-04)


### Bug Fixes

* generate nupkg, cleanup, git workflow ([05f10dd](https://github.com/informatievlaanderen/road-registry/commit/05f10ddb593f99ec282f53433e48003b7424c3c2))
* MessagingHost.Sqs had invalid AssemblyName/RootNamespace ([6d04661](https://github.com/informatievlaanderen/road-registry/commit/6d046614b0b816a85ac109d69c006d467cef941d))


### Features

* Prepare refactor for use of multiple entry points which send commands ([baccbe9](https://github.com/informatievlaanderen/road-registry/commit/baccbe9ff23f97aa21ddb06520312bb36824a270))

## [3.9.1](https://github.com/informatievlaanderen/road-registry/compare/v3.9.0...v3.9.1) (2022-10-03)


### Bug Fixes

* nupkg, pipeline, github workflows ([45d3a18](https://github.com/informatievlaanderen/road-registry/commit/45d3a18383c4018c9c7cbf3f41b205a20ff114ea))

# [3.9.0](https://github.com/informatievlaanderen/road-registry/compare/v3.8.11...v3.9.0) (2022-10-03)


### Bug Fixes

* add missing upload test cases for extracts and parameter name typos ([6c6903a](https://github.com/informatievlaanderen/road-registry/commit/6c6903a5110f103dd803976b4a969c4e90417c14))
* Await asynchronous using blocks ([958dd2e](https://github.com/informatievlaanderen/road-registry/commit/958dd2e648881241032929103eb0a2317e29b91c))
* CI tests with multiple test libs ([2df7691](https://github.com/informatievlaanderen/road-registry/commit/2df76919729122fa85d8e5b957cc9e6d8aae140e))
* cleanup code ([27b041b](https://github.com/informatievlaanderen/road-registry/commit/27b041bb2c739ae808cdef44679bc4250aa5e894))
* cleanup code with ReSharper solution settings ([90ec8dc](https://github.com/informatievlaanderen/road-registry/commit/90ec8dc063352e03af5ed5e42010efd55ce726dc))
* Configuration manager missing platforms for test projects ([1e46d53](https://github.com/informatievlaanderen/road-registry/commit/1e46d53d2b057da2c2890f61cce14dd896416110))
* Corrupted Microsoft.Data.SqlClient library upgrade ([eb5d149](https://github.com/informatievlaanderen/road-registry/commit/eb5d1494f7c182166237131bb1e0ddb48bd05c32))
* reverse lockfileVersion back to 1 ([f6c56a6](https://github.com/informatievlaanderen/road-registry/commit/f6c56a6225a1abe75482b5a1ba8a8f049b1a2827))
* Unit tests failed after separation ([76ff93b](https://github.com/informatievlaanderen/road-registry/commit/76ff93b1c4f0f42cd0dd1f24e93e8e68f1ad949c))
* Visibility marker on SqlServerCollection ([25a765b](https://github.com/informatievlaanderen/road-registry/commit/25a765b3be9a484dedbbe05a5a7cda549966345d))


### Features

* Accept cancelled tasks as valid ([c14b06e](https://github.com/informatievlaanderen/road-registry/commit/c14b06eb28c9f76f69315203d7da44f394224a32))
* add configuration for consumer delay ([4812fa5](https://github.com/informatievlaanderen/road-registry/commit/4812fa58ce2db86cc93fa099d9c545cc991f2449))
* add feature compare support for endpoint and updated tests ([a558f93](https://github.com/informatievlaanderen/road-registry/commit/a558f938fe0a55a770936582fe93655b83d726fc))
* add feature compare switch ([2297002](https://github.com/informatievlaanderen/road-registry/commit/2297002d318dcfdf3ebc713d3381f270660fbba3))
* Add missing Lambda implementation for Docker check and initialize ([63b7502](https://github.com/informatievlaanderen/road-registry/commit/63b7502c2ba5a9840dae7e734b325ae277be326a))
* Add missing Lambda implementation for Docker check and initialize ([92836a4](https://github.com/informatievlaanderen/road-registry/commit/92836a41da5af7eddfc30c370b0b78a2dc246a93))
* Add valid zip file for testing ([0992242](https://github.com/informatievlaanderen/road-registry/commit/0992242f2e2a03d6ffa4e41e3f54f3f365c30447))
* Added new style lambda project and test project ([c706bcb](https://github.com/informatievlaanderen/road-registry/commit/c706bcb1106b44c7837070f2a6e65bbb09e7e4d1))
* Corrected code style issues and sqs handler seutp ([5f32918](https://github.com/informatievlaanderen/road-registry/commit/5f3291895c20f7f928c9a5bbb75cfdce0e138059))
* create before feature compare validators ([c00f3ce](https://github.com/informatievlaanderen/road-registry/commit/c00f3ce15a09282b18c2d47d992e87b9776132ae))
* Docker containers ([e6378db](https://github.com/informatievlaanderen/road-registry/commit/e6378db7aba4080ad7fb301edbb8b70cb5c8f8ff))
* Extend validators with pipeline behavior ([77f6ecc](https://github.com/informatievlaanderen/road-registry/commit/77f6ecc536a5801fc60aa39855d9ec32aa1bb248))
* Feature compare differentiator ([e15106c](https://github.com/informatievlaanderen/road-registry/commit/e15106c2d9f2f28ce188a47a23e755dea35ca4d6))
* Handlers and validators run through ([293ac13](https://github.com/informatievlaanderen/road-registry/commit/293ac130daefb91f6280f9164126c519dd035465))
* MediatR refactor continuation ([2db1ef5](https://github.com/informatievlaanderen/road-registry/commit/2db1ef5043c3c8e245b3c57fea4cddb1f2d80747))
* Merge from main ([4126b17](https://github.com/informatievlaanderen/road-registry/commit/4126b17978e98c3a976bae6bfda49b8d58d81737))
* Merge from main ([8f0fb6f](https://github.com/informatievlaanderen/road-registry/commit/8f0fb6f36a78aeeefa221fb70a8a2758d50e0802))
* refactor solution ([4c04e9f](https://github.com/informatievlaanderen/road-registry/commit/4c04e9f6693e34c77ce7fa0c60aaa8ac3ac09373))
* Refactor test projects with additional cleanup code ([2d54fce](https://github.com/informatievlaanderen/road-registry/commit/2d54fce1b96f4d99758a11682e33b7779efee408))
* Remaining test classes which failed ([cd90aa7](https://github.com/informatievlaanderen/road-registry/commit/cd90aa738069cb26e8dd4fdf38ec3f1e601e166f))
* Remove Lambda test project ([2c32a3a](https://github.com/informatievlaanderen/road-registry/commit/2c32a3a62328116a2183a17065213db5e4086ff4))
* Removed Lambda project ([8c21b35](https://github.com/informatievlaanderen/road-registry/commit/8c21b35adefa794a331836d4d435b541daa845f5))
* set specific version for MediatR dependency ([56f53a0](https://github.com/informatievlaanderen/road-registry/commit/56f53a0692cf784b4990001da57f2a2faa92f1ab))
* Skipped uncompleted tests ([8a3ecbc](https://github.com/informatievlaanderen/road-registry/commit/8a3ecbc028a04a96d3e8f6d2072f9ec27a0e58d9))
* Solution wide code cleanup ([5bb34f3](https://github.com/informatievlaanderen/road-registry/commit/5bb34f3cc2f5361f453dd13e5b0b3254c280fd56))
* sqs consumer ([25c35af](https://github.com/informatievlaanderen/road-registry/commit/25c35afb54238bf4422ef3d3e4ae8d2b9d7173e3))
* SQS queue publisher/consumer and split unit tests ([bca2599](https://github.com/informatievlaanderen/road-registry/commit/bca25996838302c0bbcbaee85032a82940e05560))
* SQS testability ([2fe9824](https://github.com/informatievlaanderen/road-registry/commit/2fe9824ce838599b07020e230def608e2c9101da))
* Test project seperation and DotSettings cleanup ([dbf61ce](https://github.com/informatievlaanderen/road-registry/commit/dbf61ce1a2a5acc0d6b8f60964594e2d79e075dc))
* Update test cases and sql server collection ([612b647](https://github.com/informatievlaanderen/road-registry/commit/612b6476ada53f17eb3e47cbd8cc2bab6940793f))
* Update validators and fixed tests ([f6eaeba](https://github.com/informatievlaanderen/road-registry/commit/f6eaeba3f97a7d94b29398bc253ca167ae6edbd5))
* Update workflows and copy messaginghost dependencies ([e59d13b](https://github.com/informatievlaanderen/road-registry/commit/e59d13bd1167a7ace681b7b6447ed65e67c1032e))
* Updated feature compare buckets input output ([2e20836](https://github.com/informatievlaanderen/road-registry/commit/2e20836bd042dbbd14cb9fa0f16d0e8a24c499fb))
* Updated lambda and tests ([7c6c8f1](https://github.com/informatievlaanderen/road-registry/commit/7c6c8f182db4f27276bb9a316ec71b03b00828b7))
* Updated Lambda with environment variables ([97af338](https://github.com/informatievlaanderen/road-registry/commit/97af338a71251178b0d305782b4f461ee1d93130))
* Updated paket dependencies and solution update ([be4f532](https://github.com/informatievlaanderen/road-registry/commit/be4f5329f22011736cdd540e945a3de04614a610))
* updated tests for upload controller ([f5f1a10](https://github.com/informatievlaanderen/road-registry/commit/f5f1a10985017e1af57bb19f529755a1b1aeda84))
* Updated UI with feature compare ([a4a8e7a](https://github.com/informatievlaanderen/road-registry/commit/a4a8e7ad2d8c75481fad6cee016f195de05b9892))
* use port in Docker container name to allow parallel tests ([a95fbb7](https://github.com/informatievlaanderen/road-registry/commit/a95fbb7dae5eb2fd7b5865ba64ec7bb13fed771a))

## [3.8.11](https://github.com/informatievlaanderen/road-registry/compare/v3.8.10...v3.8.11) (2022-09-21)


### Bug Fixes

* make wegsegmentid not nullable id & primary key in db ([0c26fc0](https://github.com/informatievlaanderen/road-registry/commit/0c26fc0e0e3c64a66418b08647a7e084c621c952))

## [3.8.10](https://github.com/informatievlaanderen/road-registry/compare/v3.8.9...v3.8.10) (2022-09-21)


### Bug Fixes

* remove accessrestriction id column ([c9014ed](https://github.com/informatievlaanderen/road-registry/commit/c9014ed7087b0f527fb90d4e3d3204e8c434d918))

## [3.8.9](https://github.com/informatievlaanderen/road-registry/compare/v3.8.8...v3.8.9) (2022-09-21)


### Bug Fixes

* add label AccessRestriction to wfs ([e562c6b](https://github.com/informatievlaanderen/road-registry/commit/e562c6b5f8ebdecf2be5d63a279791279708f8a8))

## [3.8.8](https://github.com/informatievlaanderen/road-registry/compare/v3.8.7...v3.8.8) (2022-09-20)


### Bug Fixes

* bugfix wfs ([70f78d1](https://github.com/informatievlaanderen/road-registry/commit/70f78d18029893a021bef9ce84bb3bda43a7d189))
* translate wfs types ([d37da19](https://github.com/informatievlaanderen/road-registry/commit/d37da194bb9872d196f4797d06d8ef64978eda98))

## [3.8.7](https://github.com/informatievlaanderen/road-registry/compare/v3.8.6...v3.8.7) (2022-09-19)


### Bug Fixes

* wfs duplicate records ([ab056a3](https://github.com/informatievlaanderen/road-registry/commit/ab056a31ae1c5f88f87124b07c3f7b3f83c9ff69))

## [3.8.6](https://github.com/informatievlaanderen/road-registry/compare/v3.8.5...v3.8.6) (2022-09-14)


### Bug Fixes

* syndication response projector ([3a62be3](https://github.com/informatievlaanderen/road-registry/commit/3a62be33668c0a6c771a6e3a7ea9770c8710eec8))

## [3.8.5](https://github.com/informatievlaanderen/road-registry/compare/v3.8.4...v3.8.5) (2022-09-14)


### Bug Fixes

* syndication status endpoint response ([1b4c60b](https://github.com/informatievlaanderen/road-registry/commit/1b4c60b7234f1a3f9fb5792d12e7150a1219c1dd))

## [3.8.4](https://github.com/informatievlaanderen/road-registry/compare/v3.8.3...v3.8.4) (2022-09-13)


### Bug Fixes

* add syndication controller to projector ([9b545be](https://github.com/informatievlaanderen/road-registry/commit/9b545bebe4fdb0023f504800d20f4941829ed9cd))

## [3.8.3](https://github.com/informatievlaanderen/road-registry/compare/v3.8.2...v3.8.3) (2022-09-13)


### Bug Fixes

* register projections in projector ([e413491](https://github.com/informatievlaanderen/road-registry/commit/e413491e7b3200d7d64a730479036422fb7ae26b))

## [3.8.2](https://github.com/informatievlaanderen/road-registry/compare/v3.8.1...v3.8.2) (2022-09-13)


### Bug Fixes

* build trigger ([07d4f94](https://github.com/informatievlaanderen/road-registry/commit/07d4f9472aa64c2168d7ca8bd87343506e73c408))

## [3.8.1](https://github.com/informatievlaanderen/road-registry/compare/v3.8.0...v3.8.1) (2022-09-12)


### Bug Fixes

* build trigger ([3e850d8](https://github.com/informatievlaanderen/road-registry/commit/3e850d8f5228f33986d01ce765733a843bc4ca3a))
* update CI ([269eac2](https://github.com/informatievlaanderen/road-registry/commit/269eac2784d56adde906dbf40499dd2ad549aec4))

# [3.8.0](https://github.com/informatievlaanderen/road-registry/compare/v3.7.6...v3.8.0) (2022-09-12)


### Bug Fixes

* build.fsx file ([ea85208](https://github.com/informatievlaanderen/road-registry/commit/ea85208e54623033e3e3f9df70148259a6ff91b8))
* dockerfile projector ([b548142](https://github.com/informatievlaanderen/road-registry/commit/b548142afb41b25c53236c589e3d2af99369c37d))
* remove duplicate code ([abccbf9](https://github.com/informatievlaanderen/road-registry/commit/abccbf9f4e2e41b5dc6c3a7d17cd767f080150e4))


### Features

* create projector service for status page ([#590](https://github.com/informatievlaanderen/road-registry/issues/590)) ([0c00db7](https://github.com/informatievlaanderen/road-registry/commit/0c00db748fdff88d1912e44932b071bb1ab5e6ff))

## [3.7.6](https://github.com/informatievlaanderen/road-registry/compare/v3.7.5...v3.7.6) (2022-09-12)


### Bug Fixes

* show the error message title in the alert container ([f8e235f](https://github.com/informatievlaanderen/road-registry/commit/f8e235f26e1c0963ea707cc94a81989b8f94e5bf))

## [3.7.5](https://github.com/informatievlaanderen/road-registry/compare/v3.7.4...v3.7.5) (2022-09-07)


### Bug Fixes

* add enabled property to metadata updater ([0c0f58d](https://github.com/informatievlaanderen/road-registry/commit/0c0f58d0568c51cd8a3e7bc6562db5972105f687))
* include env var for sql server ([31348a6](https://github.com/informatievlaanderen/road-registry/commit/31348a6dee03a83e497c5db8a846a2deadd3eb91))
* include sqlserver ([6f706ba](https://github.com/informatievlaanderen/road-registry/commit/6f706badec340ea53e03368e1751d49d782242ef))
* make build.yml ci ([9629306](https://github.com/informatievlaanderen/road-registry/commit/96293065d0155c93a52611465a59a6d0cdf77e73))
* remove release check ([37922f4](https://github.com/informatievlaanderen/road-registry/commit/37922f469836fef1700ad4570ff139d552e98fc9))

## [3.7.4](https://github.com/informatievlaanderen/road-registry/compare/v3.7.3...v3.7.4) (2022-09-06)


### Bug Fixes

* don't throw Exception ([7bcc8b1](https://github.com/informatievlaanderen/road-registry/commit/7bcc8b17ff471d63fda311ea7740516eda4396c7))
* remove empty finally block ([053e41a](https://github.com/informatievlaanderen/road-registry/commit/053e41a9f88a7d5cf349db582a90b17cbce060b6))
* serializable exceptions ([7ae1e1f](https://github.com/informatievlaanderen/road-registry/commit/7ae1e1f1a5dc4c8f883d3e91a7a404d71952aca2))
* use CancellationToken ([756add2](https://github.com/informatievlaanderen/road-registry/commit/756add2cac3d705b789babb82081bd9212369582))

## [3.7.3](https://github.com/informatievlaanderen/road-registry/compare/v3.7.2...v3.7.3) (2022-09-05)


### Bug Fixes

* metadata updater ([580eb1c](https://github.com/informatievlaanderen/road-registry/commit/580eb1c1bd41ff09e778b2eaa61d4eb5a6fa5248))

## [3.7.2](https://github.com/informatievlaanderen/road-registry/compare/v3.7.1...v3.7.2) (2022-08-31)


### Bug Fixes

* metaupdater request body ([616c9ea](https://github.com/informatievlaanderen/road-registry/commit/616c9eac45f5332c1c3b8a35b019abfd3af8d26d))

## [3.7.1](https://github.com/informatievlaanderen/road-registry/compare/v3.7.0...v3.7.1) (2022-08-29)


### Bug Fixes

* build trigger ([0adb63d](https://github.com/informatievlaanderen/road-registry/commit/0adb63de117fc2a52f0d13f76854016b6f5d44c8))
* Updated validator and translator indexer ([3a5e17b](https://github.com/informatievlaanderen/road-registry/commit/3a5e17b466f20f213447dfc505f964b63cf0649c))

# [3.7.0](https://github.com/informatievlaanderen/road-registry/compare/v3.6.0...v3.7.0) (2022-08-29)


### Bug Fixes

* build trigger ([a643b3f](https://github.com/informatievlaanderen/road-registry/commit/a643b3f71b697653c64919f1a052aadb8dc4ae48))


### Features

* Support multiple versions for transactiezones.dbf ([96e6ded](https://github.com/informatievlaanderen/road-registry/commit/96e6ded1b0cfb3a1aca908494a9545edfb75f067))

# [3.6.0](https://github.com/informatievlaanderen/road-registry/compare/v3.5.6...v3.6.0) (2022-08-29)


### Bug Fixes

* add migration ([96e2028](https://github.com/informatievlaanderen/road-registry/commit/96e2028dcd360e65afd12a84a72ab07f43a79fcb))


### Features

* create wfs projection ([126e3fc](https://github.com/informatievlaanderen/road-registry/commit/126e3fcf96983cdedb77b8d36b8c94532ff14654))

## [3.5.6](https://github.com/informatievlaanderen/road-registry/compare/v3.5.5...v3.5.6) (2022-08-18)


### Bug Fixes

* expand integration layer inwards when building extracts ([c2a472c](https://github.com/informatievlaanderen/road-registry/commit/c2a472c42ec70c0d3829aeaf65b0d6915adfd6af))

## [3.5.5](https://github.com/informatievlaanderen/road-registry/compare/v3.5.4...v3.5.5) (2022-08-16)


### Bug Fixes

* road nodes integration layer now calculated on boundary of nodes instead of contour ([8af6496](https://github.com/informatievlaanderen/road-registry/commit/8af64967b5e2b40da1fa2c755772c41f94e090af))
* road segment integration layer now calculated on boundary of segments instead of contour ([92d6804](https://github.com/informatievlaanderen/road-registry/commit/92d6804107f84128e4e00a862206f25333ab1141))

## [3.5.4](https://github.com/informatievlaanderen/road-registry/compare/v3.5.3...v3.5.4) (2022-07-05)


### Bug Fixes

* add test CD ([e7755a8](https://github.com/informatievlaanderen/road-registry/commit/e7755a8ca8cc2376c54802952f8da93a4aba63cd))

## [3.5.3](https://github.com/informatievlaanderen/road-registry/compare/v3.5.2...v3.5.3) (2022-06-30)


### Bug Fixes

* modify connection strings in docker compose ([cf0916d](https://github.com/informatievlaanderen/road-registry/commit/cf0916dc3026814e135dd1863315c1f099393f2d))

## [3.5.2](https://github.com/informatievlaanderen/road-registry/compare/v3.5.1...v3.5.2) (2022-06-30)


### Bug Fixes

* add LABEL to Dockerfile (for easier DataDog filtering) ([3499146](https://github.com/informatievlaanderen/road-registry/commit/3499146e822e2df0afea5d47329ff998b1dc2faa))

## [3.5.1](https://github.com/informatievlaanderen/road-registry/compare/v3.5.0...v3.5.1) (2022-06-29)


### Bug Fixes

* import database with correct name ([04d78c2](https://github.com/informatievlaanderen/road-registry/commit/04d78c24dcd9db7957d645f47d602eb6ed0133c4))

# [3.5.0](https://github.com/informatievlaanderen/road-registry/compare/v3.4.11...v3.5.0) (2022-06-29)


### Features

* add error handling for invalid contours in ui ([b9e7642](https://github.com/informatievlaanderen/road-registry/commit/b9e7642b56b56c723928b007259e85652eb9ca19))
* add restore db functionality ([b79c77f](https://github.com/informatievlaanderen/road-registry/commit/b79c77f64f10d104e423fb5dcff3822eea311129))

## [3.4.11](https://github.com/informatievlaanderen/road-registry/compare/v3.4.10...v3.4.11) (2022-06-28)


### Bug Fixes

* remove more duplications ([6a403dd](https://github.com/informatievlaanderen/road-registry/commit/6a403ddc50ee5a84f61bdb73348b7933493270ea))

## [3.4.10](https://github.com/informatievlaanderen/road-registry/compare/v3.4.9...v3.4.10) (2022-06-27)


### Bug Fixes

* reduce duplications ([578d12b](https://github.com/informatievlaanderen/road-registry/commit/578d12b898a9e44c469cca84f1cb6fb963221104))

## [3.4.9](https://github.com/informatievlaanderen/road-registry/compare/v3.4.8...v3.4.9) (2022-06-27)


### Bug Fixes

* fix even more Sonar issues ([a4506e4](https://github.com/informatievlaanderen/road-registry/commit/a4506e41bc93ee6767c340a27a557568d2652423))

## [3.4.8](https://github.com/informatievlaanderen/road-registry/compare/v3.4.7...v3.4.8) (2022-06-27)


### Bug Fixes

* fix more Sonar issues ([40325e4](https://github.com/informatievlaanderen/road-registry/commit/40325e4760f2fbcfcaf8713f2a8b6415f17d18ce))

## [3.4.7](https://github.com/informatievlaanderen/road-registry/compare/v3.4.6...v3.4.7) (2022-06-27)


### Bug Fixes

* fix various Sonar issues ([b3dd5d9](https://github.com/informatievlaanderen/road-registry/commit/b3dd5d9026f3f0aaef9ba99526d078c95e67f14e))

## [3.4.6](https://github.com/informatievlaanderen/road-registry/compare/v3.4.5...v3.4.6) (2022-06-24)

## [3.4.5](https://github.com/informatievlaanderen/road-registry/compare/v3.4.4...v3.4.5) (2022-06-22)


### Bug Fixes

* correct file extension check when uploading extract ([c6f41db](https://github.com/informatievlaanderen/road-registry/commit/c6f41dbf0c20603b327a7e20c2fc75d038bf4a23))

## [3.4.4](https://github.com/informatievlaanderen/road-registry/compare/v3.4.3...v3.4.4) (2022-06-21)


### Bug Fixes

* remove head blob before creating new one ([3c5bdb3](https://github.com/informatievlaanderen/road-registry/commit/3c5bdb31fa0cba6deba144b63c3f04a1b7aea195))

## [3.4.3](https://github.com/informatievlaanderen/road-registry/compare/v3.4.2...v3.4.3) (2022-06-21)


### Bug Fixes

* add missing dependency configuration ([284ad55](https://github.com/informatievlaanderen/road-registry/commit/284ad551376da0206bb97e61136ab2cc92ed136a))

## [3.4.2](https://github.com/informatievlaanderen/road-registry/compare/v3.4.1...v3.4.2) (2022-06-20)


### Bug Fixes

* trigger build ([447cd33](https://github.com/informatievlaanderen/road-registry/commit/447cd339c3fd6fa0a1b59f4e808d04a21c0ab9a8))

## [3.4.1](https://github.com/informatievlaanderen/road-registry/compare/v3.4.0...v3.4.1) (2022-06-20)


### Bug Fixes

* remove basic authentication ([9202970](https://github.com/informatievlaanderen/road-registry/commit/9202970356a624c5040c9519dafec93a0f80e849))

# [3.4.0](https://github.com/informatievlaanderen/road-registry/compare/v3.3.0...v3.4.0) (2022-06-15)


### Bug Fixes

* add health check endpoint ([#532](https://github.com/informatievlaanderen/road-registry/issues/532)) ([121c6bc](https://github.com/informatievlaanderen/road-registry/commit/121c6bcaf52e058e6dc59e03c4d310b352b883c0))
* add missing dependencies ([7988711](https://github.com/informatievlaanderen/road-registry/commit/798871122a25151c913c14199deb1d0d97f36ae2))
* fix syntax error ([e742553](https://github.com/informatievlaanderen/road-registry/commit/e7425530b8955906d1055db8214248591e53b919))


### Features

* add ability to rebuild snapshots ([4b104e2](https://github.com/informatievlaanderen/road-registry/commit/4b104e2fbe3e62430fcee6010d485dd262c54d85))

# [3.3.0](https://github.com/informatievlaanderen/road-registry/compare/v3.2.6...v3.3.0) (2022-06-13)


### Features

* add feature toggle ([6f23cca](https://github.com/informatievlaanderen/road-registry/commit/6f23ccae24aa8e75cc3b8258d955b07d23e4a199))

## [3.2.6](https://github.com/informatievlaanderen/road-registry/compare/v3.2.5...v3.2.6) (2022-06-13)


### Bug Fixes

* add missing dependency to api paket template ([ab286eb](https://github.com/informatievlaanderen/road-registry/commit/ab286eba75dffdf7d913eee54f635d4f0e8c7d26))

## [3.2.5](https://github.com/informatievlaanderen/road-registry/compare/v3.2.4...v3.2.5) (2022-06-09)


### Bug Fixes

* add missing event handlers for projections ([808830a](https://github.com/informatievlaanderen/road-registry/commit/808830ae1a6d224c64ca712d67bdeeebfba8b8e2))

## [3.2.4](https://github.com/informatievlaanderen/road-registry/compare/v3.2.3...v3.2.4) (2022-06-03)


### Bug Fixes

* add missing event handlers to road network view builder ([9539042](https://github.com/informatievlaanderen/road-registry/commit/9539042a6418bff91c8b0daf73ebfe1b736e925c))

## [3.2.3](https://github.com/informatievlaanderen/road-registry/compare/v3.2.2...v3.2.3) (2022-05-31)


### Bug Fixes

* api swagger dependency verison mismatch ([9117e4e](https://github.com/informatievlaanderen/road-registry/commit/9117e4e9eb8573458fab18602a0ad6b963f191f6))

## [3.2.2](https://github.com/informatievlaanderen/road-registry/compare/v3.2.1...v3.2.2) (2022-05-30)


### Bug Fixes

* add vue comp and migrate upload page ([520ffe3](https://github.com/informatievlaanderen/road-registry/commit/520ffe3ed01d5db2a0a068241281adbc681a66e8))

## [3.2.1](https://github.com/informatievlaanderen/road-registry/compare/v3.2.0...v3.2.1) (2022-05-30)


### Bug Fixes

* fix extract requests ([0ef4f90](https://github.com/informatievlaanderen/road-registry/commit/0ef4f9013d16c5804aafd363b1f97dc3878366cb))

# [3.2.0](https://github.com/informatievlaanderen/road-registry/compare/v3.1.9...v3.2.0) (2022-05-24)


### Bug Fixes

* move login details to secure store ([1dbb953](https://github.com/informatievlaanderen/road-registry/commit/1dbb953369e2c232c4aadd5742a03ba967f54a33))


### Features

* moved login details to secure store ([c7b0d4c](https://github.com/informatievlaanderen/road-registry/commit/c7b0d4c16cbd4c75366c332edccf51305800942d))


### Reverts

* Revert "feat: moved login details to secure store" ([24164cf](https://github.com/informatievlaanderen/road-registry/commit/24164cfc1f5596bf1df4168c0cae1c8d57fd4345))

## [3.1.9](https://github.com/informatievlaanderen/road-registry/compare/v3.1.8...v3.1.9) (2022-05-23)


### Bug Fixes

* fix validation for description ([7c9f0dc](https://github.com/informatievlaanderen/road-registry/commit/7c9f0dcfe6b8896df66b0d79c32638eff1189fbf))

## [3.1.8](https://github.com/informatievlaanderen/road-registry/compare/v3.1.7...v3.1.8) (2022-05-20)


### Bug Fixes

* remove duplicates ([#512](https://github.com/informatievlaanderen/road-registry/issues/512)) ([d71ef31](https://github.com/informatievlaanderen/road-registry/commit/d71ef313e52cd7689cd247e9aa438762a549c41a))

## [3.1.7](https://github.com/informatievlaanderen/road-registry/compare/v3.1.6...v3.1.7) (2022-05-20)


### Bug Fixes

* collapse if statements ([847ca9a](https://github.com/informatievlaanderen/road-registry/commit/847ca9a8546c50cacb43d34fab060cf2c676098d))

## [3.1.6](https://github.com/informatievlaanderen/road-registry/compare/v3.1.5...v3.1.6) (2022-05-20)


### Bug Fixes

* seal private classes ([c0d0dd6](https://github.com/informatievlaanderen/road-registry/commit/c0d0dd619f1560728e974586d499da21e5954320))

## [3.1.5](https://github.com/informatievlaanderen/road-registry/compare/v3.1.4...v3.1.5) (2022-05-19)


### Bug Fixes

* fix validation errors for identical identifiers but different actions ([2eab15a](https://github.com/informatievlaanderen/road-registry/commit/2eab15afb5f26fc492f323c88b0f5c6f825c912f))

## [3.1.4](https://github.com/informatievlaanderen/road-registry/compare/v3.1.3...v3.1.4) (2022-05-16)


### Bug Fixes

* bump DotSpatial.Projections ([7edfcf9](https://github.com/informatievlaanderen/road-registry/commit/7edfcf993ecde09d00d05edd1bb0ac8546b2528c))

## [3.1.3](https://github.com/informatievlaanderen/road-registry/compare/v3.1.2...v3.1.3) (2022-05-16)


### Bug Fixes

* fix nginx conf to allow large uploads ([3079721](https://github.com/informatievlaanderen/road-registry/commit/3079721bcd19bc26c1e9daef3db6bf311c0b52a2))

## [3.1.2](https://github.com/informatievlaanderen/road-registry/compare/v3.1.1...v3.1.2) (2022-05-16)


### Bug Fixes

* remove duplicates ([2de1b7c](https://github.com/informatievlaanderen/road-registry/commit/2de1b7cd95e6a7af021d39160a3039c202eb39e6))

## [3.1.1](https://github.com/informatievlaanderen/road-registry/compare/v3.1.0...v3.1.1) (2022-05-16)


### Bug Fixes

* fix availability of extract archives when writing to s3 ([ff3a527](https://github.com/informatievlaanderen/road-registry/commit/ff3a52724ef8c876637098e3bd1182982cb568ee))

# [3.1.0](https://github.com/informatievlaanderen/road-registry/compare/v3.0.0...v3.1.0) (2022-05-13)


### Features

* only show flemish municipalities ([5778c20](https://github.com/informatievlaanderen/road-registry/commit/5778c20d84e7a04eddc99e5cdb7f339ccafe1d42))

# [3.0.0](https://github.com/informatievlaanderen/road-registry/compare/v2.1.4...v3.0.0) (2022-05-13)


### Bug Fixes

* build pipeline ([e55804e](https://github.com/informatievlaanderen/road-registry/commit/e55804e984baf35172ee1e68cf90aa80ea163d1c))
* define node version in build pipeline ([78b2131](https://github.com/informatievlaanderen/road-registry/commit/78b21316f8d5633a93d7f801b735a390872b613b))
* fix failing tests ([13aa7a2](https://github.com/informatievlaanderen/road-registry/commit/13aa7a2230501e10945e1f3ee9c7d3753874d0f2))
* remove DisableRequestSizeLimit ([6d34467](https://github.com/informatievlaanderen/road-registry/commit/6d3446751235d1088c7af1a6ba8d3ec0b384cc18))
* remove DisableRequestSizeLimit ([304bdeb](https://github.com/informatievlaanderen/road-registry/commit/304bdeb67320bc559a2218576803a7957109fbe7))
* remove duplicates ([#495](https://github.com/informatievlaanderen/road-registry/issues/495)) ([9c5ec0e](https://github.com/informatievlaanderen/road-registry/commit/9c5ec0e4553e26aa590833dfcbc46d5453e55d1f))
* upgrade node version ([7651d84](https://github.com/informatievlaanderen/road-registry/commit/7651d843990c03de01174dddae67b290f24da423))
* version bump ([bb3c1fc](https://github.com/informatievlaanderen/road-registry/commit/bb3c1fcf1a3234b10aec3b3c6084d14fb3fc7b44))


### Features

* add downloadid to transactionzones file in extracts ([a76ca0f](https://github.com/informatievlaanderen/road-registry/commit/a76ca0f6824d7fa9a722f4e128f6c1a332fe3001)), closes [#480](https://github.com/informatievlaanderen/road-registry/issues/480) [#480](https://github.com/informatievlaanderen/road-registry/issues/480)


### Reverts

* Revert "fix: remove DisableRequestSizeLimit" ([ea55772](https://github.com/informatievlaanderen/road-registry/commit/ea55772ee4ada6581b36fca10a4e3fad5a79284f))


### BREAKING CHANGES

* move to dotnet 6.0.3

* feat: move to dotnet 6.0.3
* move to dotnet 6.0.3

* chore(release): 2.0.0 [skip ci]

# [2.0.0](https://github.com/informatievlaanderen/road-registry/compare/v1.62.2...v2.0.0) (2022-04-26)

### Features

## [2.1.4](https://github.com/informatievlaanderen/road-registry/compare/v2.1.3...v2.1.4) (2022-05-02)


### Bug Fixes

* fix connectionstrings ([24e1bc2](https://github.com/informatievlaanderen/road-registry/commit/24e1bc20c06dfb792a477f270ae732aacbc9830c))

## [2.1.3](https://github.com/informatievlaanderen/road-registry/compare/v2.1.2...v2.1.3) (2022-04-29)


### Bug Fixes

* run sonar end when release version != none ([62e594a](https://github.com/informatievlaanderen/road-registry/commit/62e594ab90a82872358d0518a5c3c8e3f7aea7f5))

## [2.1.2](https://github.com/informatievlaanderen/road-registry/compare/v2.1.1...v2.1.2) (2022-04-28)


### Bug Fixes

* delete dev stuff in Home.vue & redirect to activity page ([8e13b52](https://github.com/informatievlaanderen/road-registry/commit/8e13b528a0d782e23df761cea0b6366f600a43a1))

## [2.1.1](https://github.com/informatievlaanderen/road-registry/compare/v2.1.0...v2.1.1) (2022-04-28)


### Bug Fixes

* nginx conf ([f5b8429](https://github.com/informatievlaanderen/road-registry/commit/f5b8429033b1c4678742c0c97b9a9d90ca2d4d5e))
* trigger build ([7e43a21](https://github.com/informatievlaanderen/road-registry/commit/7e43a21e2e354f62de1164fb81ef52d1bf31c695))

# [2.1.0](https://github.com/informatievlaanderen/road-registry/compare/v2.0.2...v2.1.0) (2022-04-28)


### Bug Fixes

* add NPM_TOKEN env to CI ([c4798ab](https://github.com/informatievlaanderen/road-registry/commit/c4798ab5fa003da56c544b5498b956a63f7af425))
* CI ([e9ff990](https://github.com/informatievlaanderen/road-registry/commit/e9ff99078773e1a19ab9fb3c231024d9beb19546))
* switch to new ui ([b49a42a](https://github.com/informatievlaanderen/road-registry/commit/b49a42a0ff2a772deea0ce8be6ded407da5a3a8c))
* use v2 public api, reduce api calls ([5b9dc01](https://github.com/informatievlaanderen/road-registry/commit/5b9dc01349dbcabc9fd549196f3a5e05fe1fc18c))


### Features

* add activity page ([62dc413](https://github.com/informatievlaanderen/road-registry/commit/62dc413aebb91959928ba2d40796182f1ea33543))
* add download product page ([d4409ef](https://github.com/informatievlaanderen/road-registry/commit/d4409ef83f58e2e1b91181b88a77f40816c7c750))
* add information page ([a8c5551](https://github.com/informatievlaanderen/road-registry/commit/a8c5551fde927a92d9612b66b2eac12e53005090))
* add ui for requesting extract ([e6d9b33](https://github.com/informatievlaanderen/road-registry/commit/e6d9b33b9fe9943c7e86ff60f4975a88fa9fb435))
* add upload page ([42e3316](https://github.com/informatievlaanderen/road-registry/commit/42e331639fdf78e98b2203b4581598cfc8daeb67))
* add vue support (WIP) ([b3cf957](https://github.com/informatievlaanderen/road-registry/commit/b3cf957e460a6f6a94c0f88916156673cc326085))


### Performance Improvements

* migrate to parcel to v2, add proxy, setup dev workspace, add typescript support, improve hot reload, reduce npm scripts ([0c7a15c](https://github.com/informatievlaanderen/road-registry/commit/0c7a15cb7cdfbca4bc540ff75adc2dcacbbdd86c))

## [2.0.2](https://github.com/informatievlaanderen/road-registry/compare/v2.0.1...v2.0.2) (2022-04-27)


### Bug Fixes

* redirect sonar to /dev/null ([08e4075](https://github.com/informatievlaanderen/road-registry/commit/08e4075450bcdafd7eff0ec5f60b2c959e5c415d))

## [2.0.1](https://github.com/informatievlaanderen/road-registry/compare/v2.0.0...v2.0.1) (2022-04-26)


### Bug Fixes

* remove unused ctor param ([1fe84e3](https://github.com/informatievlaanderen/road-registry/commit/1fe84e334f2898607e17aa8307ad1e21aa24af0c))

# [2.0.0](https://github.com/informatievlaanderen/road-registry/compare/v1.62.2...v2.0.0) (2022-04-26)


### Features

* move to dotnet 6.0.3 ([#480](https://github.com/informatievlaanderen/road-registry/issues/480)) ([ea93005](https://github.com/informatievlaanderen/road-registry/commit/ea93005866ab908c8234bd64574c8b7ae1da422f))


### BREAKING CHANGES

* move to dotnet 6.0.3

* feat: move to dotnet 6.0.3
* move to dotnet 6.0.3

## [1.62.2](https://github.com/informatievlaanderen/road-registry/compare/v1.62.1...v1.62.2) (2022-04-21)


### Bug Fixes

* allow duplicate identifiers for certain combinations of record types ([715d30e](https://github.com/informatievlaanderen/road-registry/commit/715d30e009736b4a3d6f3980d8c954b1741a8009))
* fix overlap of records in integration layer ([282c40c](https://github.com/informatievlaanderen/road-registry/commit/282c40c3aa65527948793b30515d2afa44d3e344))
* pin dotnet version for build ([72bff3e](https://github.com/informatievlaanderen/road-registry/commit/72bff3e4993c5e0e20721083435ef6666f3a9a11))

## [1.62.1](https://github.com/informatievlaanderen/road-registry/compare/v1.62.0...v1.62.1) (2022-03-22)


### Bug Fixes

* fix validation for unique numbers in zip archive ([fc16b81](https://github.com/informatievlaanderen/road-registry/commit/fc16b812e2815e2a19b8d17d51b153502ccb63a8))

# [1.62.0](https://github.com/informatievlaanderen/road-registry/compare/v1.61.3...v1.62.0) (2022-03-21)


### Bug Fixes

* fix adding and removing of numbered/national/european roads to road network view ([b6aa737](https://github.com/informatievlaanderen/road-registry/commit/b6aa7378a9d970f9baa971a40e42f5cb11af99a0))
* fix false positives for IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction ([ae2db2a](https://github.com/informatievlaanderen/road-registry/commit/ae2db2a1943e7fb4c76001635bc421596925a7b9))
* fix redirect after requesting extract ([1881a47](https://github.com/informatievlaanderen/road-registry/commit/1881a474c6c5ed0ec5788aca9c0421d7d8358509))
* fix ui extract downloads ([d9dbd05](https://github.com/informatievlaanderen/road-registry/commit/d9dbd05237df600be4c1ad84872fc7e97885c642))
* introduce workaround for file downloads ([cad60e4](https://github.com/informatievlaanderen/road-registry/commit/cad60e4e65afa86e24f130b8e46aa5a879da7100))
* use correct endpoint when requesting extracts ([ca7ced7](https://github.com/informatievlaanderen/road-registry/commit/ca7ced7492e3ab959da07cb71aa3ccce8db33c69))


### Features

* add buffer, integration layer for requesting extract + UI ([96088bc](https://github.com/informatievlaanderen/road-registry/commit/96088bce1b6ca9fce92d4f6fd46cd08d873bfa2b))
* CI automate API_VERSION env ([70280d3](https://github.com/informatievlaanderen/road-registry/commit/70280d397228b4e200d16b1e8f5a28862e8b4fa0))

## [1.61.3](https://github.com/informatievlaanderen/road-registry/compare/v1.61.2...v1.61.3) (2022-03-17)


### Bug Fixes

* remove empty new-line ([0286b4f](https://github.com/informatievlaanderen/road-registry/commit/0286b4f3fa43675a28379244a6f5b0a4d825c108))

## [1.61.2](https://github.com/informatievlaanderen/road-registry/compare/v1.61.1...v1.61.2) (2022-03-10)


### Bug Fixes

* style to trigger build ([b1b8efd](https://github.com/informatievlaanderen/road-registry/commit/b1b8efd83efb882001af0e4fd755dace36c87a83))

## [1.61.1](https://github.com/informatievlaanderen/road-registry/compare/v1.61.0...v1.61.1) (2022-03-10)


### Bug Fixes

* fix ui extract downloads ([a18fa76](https://github.com/informatievlaanderen/road-registry/commit/a18fa769e5b11413485c39b0543c213c4c8e05e5))

# [1.61.0](https://github.com/informatievlaanderen/road-registry/compare/v1.60.8...v1.61.0) (2022-03-09)


### Features

* CI automate API_VERSION env ([58235c8](https://github.com/informatievlaanderen/road-registry/commit/58235c8e47cca818a8394ff185ee119233230932))

## [1.60.8](https://github.com/informatievlaanderen/road-registry/compare/v1.60.7...v1.60.8) (2022-03-08)


### Bug Fixes

* introduce workaround for file downloads ([5f94b3b](https://github.com/informatievlaanderen/road-registry/commit/5f94b3b42f5baea9b75d316375d08ebb0f451760))

## [1.60.7](https://github.com/informatievlaanderen/road-registry/compare/v1.60.6...v1.60.7) (2022-03-03)


### Bug Fixes

* fix adding and removing of numbered/national/european roads to road network view ([dfe0b3e](https://github.com/informatievlaanderen/road-registry/commit/dfe0b3e2d5cb5b1f6c2734b0e0f6a3e8626c0fef))

## [1.60.6](https://github.com/informatievlaanderen/road-registry/compare/v1.60.5...v1.60.6) (2022-02-22)


### Bug Fixes

* fix redirect after requesting extract ([66392f5](https://github.com/informatievlaanderen/road-registry/commit/66392f52d67a8f8caed7c7b0134d13ee32da13c8))

## [1.60.5](https://github.com/informatievlaanderen/road-registry/compare/v1.60.4...v1.60.5) (2022-02-21)

## [1.60.4](https://github.com/informatievlaanderen/road-registry/compare/v1.60.3...v1.60.4) (2022-02-15)


### Bug Fixes

* use correct endpoint when requesting extracts ([3d78b7d](https://github.com/informatievlaanderen/road-registry/commit/3d78b7d075208a3ba223f495dba2ba5544f8316b))

## [1.60.3](https://github.com/informatievlaanderen/road-registry/compare/v1.60.2...v1.60.3) (2022-02-10)

## [1.60.2](https://github.com/informatievlaanderen/road-registry/compare/v1.60.1...v1.60.2) (2022-02-10)


### Bug Fixes

* reduce page size to avoid timeouts ([698c1eb](https://github.com/informatievlaanderen/road-registry/commit/698c1eb552189ee2b874e10129b3a66d81f3373a))

## [1.60.1](https://github.com/informatievlaanderen/road-registry/compare/v1.60.0...v1.60.1) (2022-02-09)


### Bug Fixes

* reduce stream page size to avoid timeouts ([e327a6a](https://github.com/informatievlaanderen/road-registry/commit/e327a6a7f827037937e3dbee140c1c6cfd34f95d))

# [1.60.0](https://github.com/informatievlaanderen/road-registry/compare/v1.59.5...v1.60.0) (2022-01-31)


### Features

* add buffer, integration layer for requesting extract + UI ([52f9907](https://github.com/informatievlaanderen/road-registry/commit/52f99071ddbd8d2fc8b8f0ea5add60d7fb258f0a))

## [1.59.5](https://github.com/informatievlaanderen/road-registry/compare/v1.59.4...v1.59.5) (2021-12-09)


### Bug Fixes

* fix false positives for IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction ([1000386](https://github.com/informatievlaanderen/road-registry/commit/10003867755e07e3566dedfd56a12b9365e6ca6e))

## [1.59.4](https://github.com/informatievlaanderen/road-registry/compare/v1.59.3...v1.59.4) (2021-11-26)


### Bug Fixes

* correctly handle geometries with holes ([9afb924](https://github.com/informatievlaanderen/road-registry/commit/9afb924cdb754a208cf43d85b00f4fa67f7a381a))

## [1.59.3](https://github.com/informatievlaanderen/road-registry/compare/v1.59.2...v1.59.3) (2021-11-26)


### Bug Fixes

* fix handling of polygons with holes ([4517678](https://github.com/informatievlaanderen/road-registry/commit/45176783f8291055ceac3e63377d3322f57a1713))

## [1.59.2](https://github.com/informatievlaanderen/road-registry/compare/v1.59.1...v1.59.2) (2021-11-17)


### Bug Fixes

* mark nodes that are too close as warnings instead of errors ([b0578c7](https://github.com/informatievlaanderen/road-registry/commit/b0578c713b75f871427d7ffc1895d0cd5dd6bfe9))

## [1.59.1](https://github.com/informatievlaanderen/road-registry/compare/v1.59.0...v1.59.1) (2021-11-10)


### Bug Fixes

* mark nodes that are too close together as a warning instead of an error ([ee8441e](https://github.com/informatievlaanderen/road-registry/commit/ee8441ea4e331fc92e5a6e0ac604f3b7b1fe4e96))

# [1.59.0](https://github.com/informatievlaanderen/road-registry/compare/v1.58.6...v1.59.0) (2021-10-28)


### Features

* add endpoint to create extract by contour ([2b65441](https://github.com/informatievlaanderen/road-registry/commit/2b6544112c7225b02beba991e947b191356b58e5))
* add endpoint to create extract by nis ([41572a9](https://github.com/informatievlaanderen/road-registry/commit/41572a917970d6f41a87209e54f47b11d3f5ab2b))

## [1.58.6](https://github.com/informatievlaanderen/road-registry/compare/v1.58.5...v1.58.6) (2021-10-21)


### Bug Fixes

* accept 'unknown' ordinal when adding road segment to numbered road ([d58cbdc](https://github.com/informatievlaanderen/road-registry/commit/d58cbdc4bbd7e08da8f54cd5e4bfad06d9acb115))

## [1.58.5](https://github.com/informatievlaanderen/road-registry/compare/v1.58.4...v1.58.5) (2021-10-14)


### Bug Fixes

* download zip file in activity page ([2cc6c70](https://github.com/informatievlaanderen/road-registry/commit/2cc6c7047fb43832d3c434f048ca07beb47eb4ff))

## [1.58.4](https://github.com/informatievlaanderen/road-registry/compare/v1.58.3...v1.58.4) (2021-10-13)


### Bug Fixes

* download-product url ([5db0eca](https://github.com/informatievlaanderen/road-registry/commit/5db0ecaddcde1f5212e6f542941d7b76103f7383))

## [1.58.3](https://github.com/informatievlaanderen/road-registry/compare/v1.58.2...v1.58.3) (2021-10-12)


### Bug Fixes

* update docker compose file ([ba0bd80](https://github.com/informatievlaanderen/road-registry/commit/ba0bd80a9b35c2451f0e7ea9a8dba4c23ead0714))
* use old endpoints for some calls ([17bce59](https://github.com/informatievlaanderen/road-registry/commit/17bce599e266f47c0363d379c66905389607143d))

## [1.58.2](https://github.com/informatievlaanderen/road-registry/compare/v1.58.1...v1.58.2) (2021-10-12)


### Bug Fixes

* crosorigin manifest.js ([576d78f](https://github.com/informatievlaanderen/road-registry/commit/576d78fcc4bdc94a538c1176f09bfc911b76b20d))

## [1.58.1](https://github.com/informatievlaanderen/road-registry/compare/v1.58.0...v1.58.1) (2021-10-12)


### Bug Fixes

* build ([3e420c3](https://github.com/informatievlaanderen/road-registry/commit/3e420c35d0df8da59545513cfa5afa906b53ac1b))

# [1.58.0](https://github.com/informatievlaanderen/road-registry/compare/v1.57.7...v1.58.0) (2021-10-12)


### Bug Fixes

* build ([bab1dd1](https://github.com/informatievlaanderen/road-registry/commit/bab1dd15a0bdaca82e2b6c105b8c83e93a093fb1))
* fix wrong parameters ([51c7a6b](https://github.com/informatievlaanderen/road-registry/commit/51c7a6bd84a182be5b0befa63eda949a614bc885))
* optimize build-activity ([d5f102a](https://github.com/informatievlaanderen/road-registry/commit/d5f102a78677a7267272b73d3034ca9c58707709))


### Features

* use public api endpoints ([ab4a403](https://github.com/informatievlaanderen/road-registry/commit/ab4a403a331ba02bd0e9f31c9fb14a60251242de))

## [1.57.7](https://github.com/informatievlaanderen/road-registry/compare/v1.57.6...v1.57.7) (2021-10-11)


### Bug Fixes

* also build for msil ([1d81e0c](https://github.com/informatievlaanderen/road-registry/commit/1d81e0c8147cc32bd82635840779c89ee4cf538d))

## [1.57.6](https://github.com/informatievlaanderen/road-registry/compare/v1.57.5...v1.57.6) (2021-10-06)


### Bug Fixes

* add Test to ECR ([9d3541d](https://github.com/informatievlaanderen/road-registry/commit/9d3541d085904c2959acab0a2f04edb2f56fed22))
* fix failing tests for validation errors ([607d9ac](https://github.com/informatievlaanderen/road-registry/commit/607d9ace969c78d3a050c2c0705c7d994fbb5b34))
* fix failing tests for validation errors ([25dc2ab](https://github.com/informatievlaanderen/road-registry/commit/25dc2ab629f3445a81f962cb92d906c6a864d934))

## [1.57.5](https://github.com/informatievlaanderen/road-registry/compare/v1.57.4...v1.57.5) (2021-10-04)


### Bug Fixes

* add missing dependency to api paket template ([afbd063](https://github.com/informatievlaanderen/road-registry/commit/afbd0638e0d844dc44c014133b730bbbe7a88bfc))

## [1.57.4](https://github.com/informatievlaanderen/road-registry/compare/v1.57.3...v1.57.4) (2021-10-01)


### Bug Fixes

* fix upload status endpoint ([5cbed9b](https://github.com/informatievlaanderen/road-registry/commit/5cbed9b935bee5c20be421ddbaffebb21b92eaec))

## [1.57.3](https://github.com/informatievlaanderen/road-registry/compare/v1.57.2...v1.57.3) (2021-09-29)


### Bug Fixes

* add index on streetnamecacheposition WR-260 ([f6e41a8](https://github.com/informatievlaanderen/road-registry/commit/f6e41a8229875b95b97302e3e8ca89f7f1da3312))

## [1.57.2](https://github.com/informatievlaanderen/road-registry/compare/v1.57.1...v1.57.2) (2021-09-29)


### Bug Fixes

* add missing dependencies to backoffice.api nuget ([10b9881](https://github.com/informatievlaanderen/road-registry/commit/10b9881250b588ff445f857993f65a24309454ca))

## [1.57.1](https://github.com/informatievlaanderen/road-registry/compare/v1.57.0...v1.57.1) (2021-09-29)


### Bug Fixes

* add nuget push package to build ([c64a284](https://github.com/informatievlaanderen/road-registry/commit/c64a284f7bbda61356d5de904afbccd861263d3b))
* correct workflow to push to nuget ([972e24e](https://github.com/informatievlaanderen/road-registry/commit/972e24e45be5b242eb3f984f5860b491fc38ed44))

# [1.57.0](https://github.com/informatievlaanderen/road-registry/compare/v1.56.0...v1.57.0) (2021-09-29)


### Bug Fixes

* mark paket.template as content ([91df799](https://github.com/informatievlaanderen/road-registry/commit/91df7993c15da931e025a89ba74890801c1f2d6f))


### Features

* expose backoffice.api as nuget package ([4c74ba6](https://github.com/informatievlaanderen/road-registry/commit/4c74ba6c6b289ee9a90c07b37145bd9387daba14))

# [1.56.0](https://github.com/informatievlaanderen/road-registry/compare/v1.55.0...v1.56.0) (2021-09-29)


### Bug Fixes

* wr-233 add api-key-header to requests ([4ee2ef1](https://github.com/informatievlaanderen/road-registry/commit/4ee2ef1dbcceed508ec422dd051b74bf80a2b16e))


### Features

* wr-233 add x-api-key header to requests ([34ee237](https://github.com/informatievlaanderen/road-registry/commit/34ee2371ac0639eef7a29ea3efd6736e42bcc6f0))

# [1.55.0](https://github.com/informatievlaanderen/road-registry/compare/v1.54.0...v1.55.0) (2021-09-21)


### Features

* add support for 'not known' (-8) volgnummer in numbered roads ([#327](https://github.com/informatievlaanderen/road-registry/issues/327)) ([2353635](https://github.com/informatievlaanderen/road-registry/commit/23536353a0136c3c0cad41f5bf94d33a68443d3d))

# [1.54.0](https://github.com/informatievlaanderen/road-registry/compare/v1.53.0...v1.54.0) (2021-09-17)


### Bug Fixes

* add index on SSS Messages.StreamIdInternal ([a388b69](https://github.com/informatievlaanderen/road-registry/commit/a388b69e5357bc7b1a38e4dde6a1beccb9b2fea7))
* fix failing test for 680299d ([3d8b8be](https://github.com/informatievlaanderen/road-registry/commit/3d8b8bec7233abcd32000afaee815c76ce6bc6f3))


### Features

* when no changes are uploaded, give appropiate message WR-226 ([680299d](https://github.com/informatievlaanderen/road-registry/commit/680299de2c87b77d6610812c16fd783218fdb2c8))

# [1.53.0](https://github.com/informatievlaanderen/road-registry/compare/v1.52.1...v1.53.0) (2021-09-16)


### Features

* Wr 243/intersecting road segment check ([#317](https://github.com/informatievlaanderen/road-registry/issues/317)) ([ebb8ba7](https://github.com/informatievlaanderen/road-registry/commit/ebb8ba7b7bd970e9a2bf3dc85c700e63a5cb1257))

## [1.52.1](https://github.com/informatievlaanderen/road-registry/compare/v1.52.0...v1.52.1) (2021-09-07)


### Bug Fixes

* add translation for incorrect projection format ([9b0be45](https://github.com/informatievlaanderen/road-registry/commit/9b0be45886aa1de3361000ecc0ef183db1a2bbbf))

# [1.52.0](https://github.com/informatievlaanderen/road-registry/compare/v1.51.3...v1.52.0) (2021-09-02)


### Features

* check missing prj files ([#313](https://github.com/informatievlaanderen/road-registry/issues/313)) ([a2200a9](https://github.com/informatievlaanderen/road-registry/commit/a2200a97f0e06eb123fefaa38e3ed021ce512eb3))

## [1.51.3](https://github.com/informatievlaanderen/road-registry/compare/v1.51.2...v1.51.3) (2021-08-30)


### Bug Fixes

* beschrijv field of transactionzones no longer allows empty strings ([0308e66](https://github.com/informatievlaanderen/road-registry/commit/0308e66f5c372e02871497d2010dd7a186985b7d))

## [1.51.2](https://github.com/informatievlaanderen/road-registry/compare/v1.51.1...v1.51.2) (2021-08-19)


### Bug Fixes

* better sql timeout handling ([#309](https://github.com/informatievlaanderen/road-registry/issues/309)) ([e73dbac](https://github.com/informatievlaanderen/road-registry/commit/e73dbacf84b96bc06f8aa560e15f0efc682f672e))

## [1.51.1](https://github.com/informatievlaanderen/road-registry/compare/v1.51.0...v1.51.1) (2021-08-19)


### Bug Fixes

* begin and end road id should differ ([de208b8](https://github.com/informatievlaanderen/road-registry/commit/de208b8c2afa414768a9966b054a0af66d128086))

# [1.51.0](https://github.com/informatievlaanderen/road-registry/compare/v1.50.0...v1.51.0) (2021-08-17)


### Features

* ack not all changes are extract uploads ([#306](https://github.com/informatievlaanderen/road-registry/issues/306)) ([2a31083](https://github.com/informatievlaanderen/road-registry/commit/2a3108362b72d65d1fff6098485ccc16902be846))

# [1.50.0](https://github.com/informatievlaanderen/road-registry/compare/v1.49.1...v1.50.0) (2021-08-17)


### Features

* support polygon or multipolygon contour ([#274](https://github.com/informatievlaanderen/road-registry/issues/274)) ([bdda887](https://github.com/informatievlaanderen/road-registry/commit/bdda88718b650806d472e6a728ab6766f6214d21))

## [1.49.1](https://github.com/informatievlaanderen/road-registry/compare/v1.49.0...v1.49.1) (2021-08-17)


### Bug Fixes

* check value range of left and right street name ([#303](https://github.com/informatievlaanderen/road-registry/issues/303)) ([33b0381](https://github.com/informatievlaanderen/road-registry/commit/33b038120007f0ef5625e1e8abb67dae0278aea3))
* problem name was wrong ([#304](https://github.com/informatievlaanderen/road-registry/issues/304)) ([00bde95](https://github.com/informatievlaanderen/road-registry/commit/00bde95502b0d22efb6901508e308981ebca1cec))
* retry after calc chokes on empty seq ([#305](https://github.com/informatievlaanderen/road-registry/issues/305)) ([f6311f4](https://github.com/informatievlaanderen/road-registry/commit/f6311f48421fc0aa5469d42e450e7b7e9428fca6))

# [1.49.0](https://github.com/informatievlaanderen/road-registry/compare/v1.48.3...v1.49.0) (2021-08-17)


### Features

* support for extract uploads ([#281](https://github.com/informatievlaanderen/road-registry/issues/281)) ([d03913a](https://github.com/informatievlaanderen/road-registry/commit/d03913a1531efc40fdba848ed1a9e721b16969c0))

## [1.48.3](https://github.com/informatievlaanderen/road-registry/compare/v1.48.2...v1.48.3) (2021-08-10)


### Bug Fixes

* validate from position < to position ([8750280](https://github.com/informatievlaanderen/road-registry/commit/8750280772a8eec8c1a8044bf3c720abfd60758c))

## [1.48.2](https://github.com/informatievlaanderen/road-registry/compare/v1.48.1...v1.48.2) (2021-08-10)


### Bug Fixes

* validate from position < to position ([28e367a](https://github.com/informatievlaanderen/road-registry/commit/28e367a4ded55943cf8a3d9549e4551ce4fbd054))

## [1.48.1](https://github.com/informatievlaanderen/road-registry/compare/v1.48.0...v1.48.1) (2021-08-05)


### Bug Fixes

* use correct parameter index ([0b1d111](https://github.com/informatievlaanderen/road-registry/commit/0b1d1117920e6b1c78b8aaa716f9ab34bd34c4f2))

# [1.48.0](https://github.com/informatievlaanderen/road-registry/compare/v1.47.0...v1.48.0) (2021-08-05)


### Features

* use local mock server for metadata updates ([a79ebab](https://github.com/informatievlaanderen/road-registry/commit/a79ebabc2a9fbd6ac27f23d765f6a73aca78ed49))

# [1.47.0](https://github.com/informatievlaanderen/road-registry/compare/v1.46.0...v1.47.0) (2021-08-03)


### Bug Fixes

* rename and add missing municipality event types ([c45a655](https://github.com/informatievlaanderen/road-registry/commit/c45a6553617b581d7e706efd841aa715af67f104))


### Features

* inject serializers specific to the AtomFeedProcessor ([a7be207](https://github.com/informatievlaanderen/road-registry/commit/a7be207733b3ea7a89f2021e1a99a68fcaa4a15e))
* update streetname and muni dependencies in docker compose ([06f88af](https://github.com/informatievlaanderen/road-registry/commit/06f88af6a30ee16c585768d34818e14503d7fa07))

# [1.46.0](https://github.com/informatievlaanderen/road-registry/compare/v1.45.2...v1.46.0) (2021-08-03)


### Features

* log when not able to create envelope ([9d89f7e](https://github.com/informatievlaanderen/road-registry/commit/9d89f7e807c1ef7955895a047f5622cfc7fe1f66))

## [1.45.2](https://github.com/informatievlaanderen/road-registry/compare/v1.45.1...v1.45.2) (2021-08-03)


### Bug Fixes

* make sure last page is processed as well ([09af6e4](https://github.com/informatievlaanderen/road-registry/commit/09af6e47c0ec3944db4d80b446174a30e584e103))

## [1.45.1](https://github.com/informatievlaanderen/road-registry/compare/v1.45.0...v1.45.1) (2021-08-03)


### Bug Fixes

* remove e prefix from tx zones ([#282](https://github.com/informatievlaanderen/road-registry/issues/282)) ([ef0ec8e](https://github.com/informatievlaanderen/road-registry/commit/ef0ec8e9e01c47de9711b2e4a786fe41cd5409a6))

# [1.45.0](https://github.com/informatievlaanderen/road-registry/compare/v1.44.1...v1.45.0) (2021-07-30)


### Features

* update wms metadata ([#257](https://github.com/informatievlaanderen/road-registry/issues/257)) ([00e0d95](https://github.com/informatievlaanderen/road-registry/commit/00e0d954e0fc87711f1eaafae0b7d5c18b1efdc4))

## [1.44.1](https://github.com/informatievlaanderen/road-registry/compare/v1.44.0...v1.44.1) (2021-07-29)


### Bug Fixes

* return error on empty maintainer ([1c803e1](https://github.com/informatievlaanderen/road-registry/commit/1c803e1e3e5714306e6dec68352f55a178939d82))

# [1.44.0](https://github.com/informatievlaanderen/road-registry/compare/v1.43.0...v1.44.0) (2021-07-19)


### Features

* prefix with e and transaction zone support ([#273](https://github.com/informatievlaanderen/road-registry/issues/273)) ([32ed136](https://github.com/informatievlaanderen/road-registry/commit/32ed1369ca61adb68deab384742ecc6873d4e6b7))

# [1.43.0](https://github.com/informatievlaanderen/road-registry/compare/v1.42.0...v1.43.0) (2021-07-16)


### Features

* ability to download extract ([#272](https://github.com/informatievlaanderen/road-registry/issues/272)) ([04e59c5](https://github.com/informatievlaanderen/road-registry/commit/04e59c546b15a21435526ef8f491dcc016fce5cd))

# [1.42.0](https://github.com/informatievlaanderen/road-registry/compare/v1.41.0...v1.42.0) (2021-07-14)


### Features

* extract assembly ([#271](https://github.com/informatievlaanderen/road-registry/issues/271)) ([c2a2eec](https://github.com/informatievlaanderen/road-registry/commit/c2a2eec7d0981d28722f6ea5e1e5048f28f524c9))

# [1.41.0](https://github.com/informatievlaanderen/road-registry/compare/v1.40.0...v1.41.0) (2021-07-13)


### Features

* change from ok to accepted status code ([#262](https://github.com/informatievlaanderen/road-registry/issues/262)) ([8524f9f](https://github.com/informatievlaanderen/road-registry/commit/8524f9fb4fa203703704d5d4af2104f2be3b4379))

# [1.40.0](https://github.com/informatievlaanderen/road-registry/compare/v1.39.0...v1.40.0) (2021-07-13)


### Features

* make extracthost part of gh workflow ([#261](https://github.com/informatievlaanderen/road-registry/issues/261)) ([d690d43](https://github.com/informatievlaanderen/road-registry/commit/d690d4383ceda62da790c194398c8a5810415496))

# [1.39.0](https://github.com/informatievlaanderen/road-registry/compare/v1.38.1...v1.39.0) (2021-07-13)


### Features

* wr-199 extract request ([#260](https://github.com/informatievlaanderen/road-registry/issues/260)) ([ce9c64f](https://github.com/informatievlaanderen/road-registry/commit/ce9c64f73c397d6ed418c6a6a5650abcaef31d64))

## [1.38.1](https://github.com/informatievlaanderen/road-registry/compare/v1.38.0...v1.38.1) (2021-07-01)


### Bug Fixes

* bump actions/setup-node from 2.1.5 to 2.2.0 (chore) + trigger build for 31a715 ([fb8b964](https://github.com/informatievlaanderen/road-registry/commit/fb8b964b5a9e511786db6358f901f0d7a88d52e2))

# [1.38.0](https://github.com/informatievlaanderen/road-registry/compare/v1.37.2...v1.38.0) (2021-06-30)


### Features

* ignore the field offset in all uploaded dbase file headers ([#246](https://github.com/informatievlaanderen/road-registry/issues/246)) ([ea5bdd9](https://github.com/informatievlaanderen/road-registry/commit/ea5bdd9980297c74a491678e56d95437f22dda93))
* upgrade net core ([#244](https://github.com/informatievlaanderen/road-registry/issues/244)) ([5a68b9e](https://github.com/informatievlaanderen/road-registry/commit/5a68b9e1b851179aa92bdb9b7e38fd7767b9146a))

## [1.37.2](https://github.com/informatievlaanderen/road-registry/compare/v1.37.1...v1.37.2) (2021-06-15)


### Bug Fixes

* remove out of bounds call to problem params ([509fd73](https://github.com/informatievlaanderen/road-registry/commit/509fd7390cbe2ecc7f0bacc495072280c79229fd))

## [1.37.1](https://github.com/informatievlaanderen/road-registry/compare/v1.37.0...v1.37.1) (2021-06-14)


### Bug Fixes

* provide authless health check location ([116b914](https://github.com/informatievlaanderen/road-registry/commit/116b914654b40a460869e75694af96ceaa96282c))

# [1.37.0](https://github.com/informatievlaanderen/road-registry/compare/v1.36.0...v1.37.0) (2021-05-27)


### Features

* improved road node type checking ([#229](https://github.com/informatievlaanderen/road-registry/issues/229)) ([6f359af](https://github.com/informatievlaanderen/road-registry/commit/6f359af56dae9466c57ca5f723f74f799dbe15b0))

# [1.36.0](https://github.com/informatievlaanderen/road-registry/compare/v1.35.0...v1.36.0) (2021-05-26)


### Features

* treat fake node eligibility as warning ([#228](https://github.com/informatievlaanderen/road-registry/issues/228)) ([00c6895](https://github.com/informatievlaanderen/road-registry/commit/00c68956883c38739eaa547f6bf2cf472b805fa8))

# [1.35.0](https://github.com/informatievlaanderen/road-registry/compare/v1.34.1...v1.35.0) (2021-05-21)


### Features

* enrich road segment changes with org name ([#225](https://github.com/informatievlaanderen/road-registry/issues/225)) ([cd98b5d](https://github.com/informatievlaanderen/road-registry/commit/cd98b5dc07cd61d576e961c57d55fded8109550d))

## [1.34.1](https://github.com/informatievlaanderen/road-registry/compare/v1.34.0...v1.34.1) (2021-05-21)


### Bug Fixes

* do not change opndatum when road segments got modified ([#224](https://github.com/informatievlaanderen/road-registry/issues/224)) ([f0749c4](https://github.com/informatievlaanderen/road-registry/commit/f0749c459b71e4d4321e285ab22cb3da5ee400cf))

# [1.34.0](https://github.com/informatievlaanderen/road-registry/compare/v1.33.2...v1.34.0) (2021-05-13)


### Features

* bump aws mutex to fix ResourceInUseException ([#204](https://github.com/informatievlaanderen/road-registry/issues/204)) ([34bf90d](https://github.com/informatievlaanderen/road-registry/commit/34bf90df82698552478259f5b75d872ab9a8d630))
* product download improvements ([#202](https://github.com/informatievlaanderen/road-registry/issues/202)) ([e0937fb](https://github.com/informatievlaanderen/road-registry/commit/e0937fbc3a73bf04cfb38690293cc962a259334d))

## [1.33.2](https://github.com/informatievlaanderen/road-registry/compare/v1.33.1...v1.33.2) (2021-05-12)


### Bug Fixes

* dutch translation offset issue ([#221](https://github.com/informatievlaanderen/road-registry/issues/221)) ([0968f97](https://github.com/informatievlaanderen/road-registry/commit/0968f977c48beb5d958fe43a17f73df66f9aa6f4))

## [1.33.1](https://github.com/informatievlaanderen/road-registry/compare/v1.33.0...v1.33.1) (2021-05-11)


### Bug Fixes

* wrong parameter in problem message ([#206](https://github.com/informatievlaanderen/road-registry/issues/206)) ([6ac1684](https://github.com/informatievlaanderen/road-registry/commit/6ac1684169d37f4c94f2501aff33b7b610d2af12))

# [1.33.0](https://github.com/informatievlaanderen/road-registry/compare/v1.32.1...v1.33.0) (2021-04-29)


### Features

* support for validation context ([#201](https://github.com/informatievlaanderen/road-registry/issues/201)) ([4967f01](https://github.com/informatievlaanderen/road-registry/commit/4967f016aa54754a2af28c4225d3f300a3a613a1))

## [1.32.1](https://github.com/informatievlaanderen/road-registry/compare/v1.32.0...v1.32.1) (2021-04-07)


### Bug Fixes

* use 2part naming scheme for azure db ([c81cf81](https://github.com/informatievlaanderen/road-registry/commit/c81cf814c1faaf7997ba774cfaf6901b11ab48c1))

# [1.32.0](https://github.com/informatievlaanderen/road-registry/compare/v1.31.0...v1.32.0) (2021-04-01)


### Bug Fixes

* numbering issue ([6686b75](https://github.com/informatievlaanderen/road-registry/commit/6686b75dd7b480494c6499f03aeb71056dd876c1))


### Features

* explicit provisional change handling ([#188](https://github.com/informatievlaanderen/road-registry/issues/188)) ([4dc98f3](https://github.com/informatievlaanderen/road-registry/commit/4dc98f39134c0b5512521cd9be01422d796647b8))

# [1.31.0](https://github.com/informatievlaanderen/road-registry/compare/v1.30.0...v1.31.0) (2021-04-01)


### Features

* add indices to wegsegmentDenorm ([#166](https://github.com/informatievlaanderen/road-registry/issues/166)) ([c3e9fdf](https://github.com/informatievlaanderen/road-registry/commit/c3e9fdf0e71459dfe01af17b9857ff0e6ad97f8e))

# [1.30.0](https://github.com/informatievlaanderen/road-registry/compare/v1.29.0...v1.30.0) (2021-03-19)


### Bug Fixes

* missing translation for organization id ([#180](https://github.com/informatievlaanderen/road-registry/issues/180)) ([98434f3](https://github.com/informatievlaanderen/road-registry/commit/98434f3f576b605d0d349f7019bced79b4c4f228))


### Features

* fix broken tests ([#184](https://github.com/informatievlaanderen/road-registry/issues/184)) ([2fa64e8](https://github.com/informatievlaanderen/road-registry/commit/2fa64e8bbb6fffcf6cc3f8b8f99afb7f412f76c7))
* fixes road segment count upon remove ([#183](https://github.com/informatievlaanderen/road-registry/issues/183)) ([2f95522](https://github.com/informatievlaanderen/road-registry/commit/2f95522d60e23c90ff8494940ab8672a26e92a01))

# [1.29.0](https://github.com/informatievlaanderen/road-registry/compare/v1.28.0...v1.29.0) (2021-03-18)


### Features

* support x-zip-compressed as a mime type for uploading ([#181](https://github.com/informatievlaanderen/road-registry/issues/181)) ([3732a7c](https://github.com/informatievlaanderen/road-registry/commit/3732a7cbdd7d11fe1d267538ff8852c79b6b6b12))

# [1.28.0](https://github.com/informatievlaanderen/road-registry/compare/v1.27.0...v1.28.0) (2021-03-16)


### Features

* various improvements ([#170](https://github.com/informatievlaanderen/road-registry/issues/170)) ([5f6e4cc](https://github.com/informatievlaanderen/road-registry/commit/5f6e4cc13d5ea5a976bf00c21f2cc79f31929174))

# [1.27.0](https://github.com/informatievlaanderen/road-registry/compare/v1.26.0...v1.27.0) (2021-03-06)


### Features

* push wms host ([#171](https://github.com/informatievlaanderen/road-registry/issues/171)) ([56ef246](https://github.com/informatievlaanderen/road-registry/commit/56ef2468864957bbb435cb4a34fbd086e171b696))

# [1.26.0](https://github.com/informatievlaanderen/road-registry/compare/v1.25.0...v1.26.0) (2021-03-03)


### Features

* auto cancel previous download ([#169](https://github.com/informatievlaanderen/road-registry/issues/169)) ([9ce3a97](https://github.com/informatievlaanderen/road-registry/commit/9ce3a97601f57b9793f421828cdfdd17643245ce))

# [1.25.0](https://github.com/informatievlaanderen/road-registry/compare/v1.24.0...v1.25.0) (2021-03-02)


### Features

* push syndication docker image ([#168](https://github.com/informatievlaanderen/road-registry/issues/168)) ([3804d57](https://github.com/informatievlaanderen/road-registry/commit/3804d579811b7d124b013f04a36b184a181aab9d))

# [1.24.0](https://github.com/informatievlaanderen/road-registry/compare/v1.23.0...v1.24.0) (2021-03-02)


### Features

* log connectionstring upon api startup ([#167](https://github.com/informatievlaanderen/road-registry/issues/167)) ([706c945](https://github.com/informatievlaanderen/road-registry/commit/706c945067342dc8ee964e15bdf28dae7da474b5))

# [1.23.0](https://github.com/informatievlaanderen/road-registry/compare/v1.22.0...v1.23.0) (2021-03-01)


### Bug Fixes

* organization id tests ([#164](https://github.com/informatievlaanderen/road-registry/issues/164)) ([dcbe301](https://github.com/informatievlaanderen/road-registry/commit/dcbe301a8fb8bccded8589fb1299f072e970484b))


### Features

* alpha release ([#163](https://github.com/informatievlaanderen/road-registry/issues/163)) ([5bff316](https://github.com/informatievlaanderen/road-registry/commit/5bff316b775a5db765a259e1d4057a2ae3ef5ada))

# [1.22.0](https://github.com/informatievlaanderen/road-registry/compare/v1.21.2...v1.22.0) (2021-02-19)


### Features

* prepare release ([#154](https://github.com/informatievlaanderen/road-registry/issues/154)) ([53152bb](https://github.com/informatievlaanderen/road-registry/commit/53152bbb35c8e384c8b8245e46a0bf187b77e0e8))

## [1.21.2](https://github.com/informatievlaanderen/road-registry/compare/v1.21.1...v1.21.2) (2021-02-17)


### Bug Fixes

* data corruption bug ([#151](https://github.com/informatievlaanderen/road-registry/issues/151)) ([af5147d](https://github.com/informatievlaanderen/road-registry/commit/af5147d150326cc832d7d4bfb142c2447ef20ef0))

## [1.21.1](https://github.com/informatievlaanderen/road-registry/compare/v1.21.0...v1.21.1) (2021-02-16)

# [1.21.0](https://github.com/informatievlaanderen/road-registry/compare/v1.20.0...v1.21.0) (2021-02-15)


### Features

* emit projection format files ([08620ab](https://github.com/informatievlaanderen/road-registry/commit/08620aba4f9dcb24fb74d617018e8dc08c6bb0a6))

# [1.20.0](https://github.com/informatievlaanderen/road-registry/compare/v1.19.0...v1.20.0) (2021-02-04)


### Features

* support paging thru the change feed ([#125](https://github.com/informatievlaanderen/road-registry/issues/125)) ([9e8d140](https://github.com/informatievlaanderen/road-registry/commit/9e8d140a2532f81690b528d63093131903e3f06e))

# [1.19.0](https://github.com/informatievlaanderen/road-registry/compare/v1.18.0...v1.19.0) (2021-02-04)


### Features

* update syndication images ([c525e5a](https://github.com/informatievlaanderen/road-registry/commit/c525e5a792d48773a3933a8f3a8ea4563f744f77))

# [1.18.0](https://github.com/informatievlaanderen/road-registry/compare/v1.17.1...v1.18.0) (2021-02-04)


### Features

* apply new events ([#130](https://github.com/informatievlaanderen/road-registry/issues/130)) ([3277ea6](https://github.com/informatievlaanderen/road-registry/commit/3277ea607da97a1a981c54ecf03d911ac41e86e4))

## [1.17.1](https://github.com/informatievlaanderen/road-registry/compare/v1.17.0...v1.17.1) (2021-02-03)


### Bug Fixes

* move to 5.0.2 ([f09ca4f](https://github.com/informatievlaanderen/road-registry/commit/f09ca4f07acbb12a379beb2ded7f038e96aa70ed))
* upgrade to latest csvhelper ([1a305ad](https://github.com/informatievlaanderen/road-registry/commit/1a305ade1b0b66b131a5b737de1554e9e8543de5))

# [1.17.0](https://github.com/informatievlaanderen/road-registry/compare/v1.16.0...v1.17.0) (2021-02-02)


### Features

* basic diagrams to explain information flow ([#129](https://github.com/informatievlaanderen/road-registry/issues/129)) ([713a56f](https://github.com/informatievlaanderen/road-registry/commit/713a56fbbb2900c4b89ddc1a5ca228d5db20dd87))

# [1.16.0](https://github.com/informatievlaanderen/road-registry/compare/v1.15.1...v1.16.0) (2021-01-08)


### Bug Fixes

* move to net 5.0.1 ([#110](https://github.com/informatievlaanderen/road-registry/issues/110)) ([8387532](https://github.com/informatievlaanderen/road-registry/commit/8387532aeb55c98fc89a0abd106a2ce07f9b62e0))
* various upon testing with real data ([#82](https://github.com/informatievlaanderen/road-registry/issues/82)) ([2fcba09](https://github.com/informatievlaanderen/road-registry/commit/2fcba092e93f4728780e4ee505287d5822fcd629))


### Features

* extract import municipalities ([#101](https://github.com/informatievlaanderen/road-registry/issues/101)) ([fd6a5a8](https://github.com/informatievlaanderen/road-registry/commit/fd6a5a8fe1200a245cece53d7355af2967d093a8))

## [1.15.1](https://github.com/informatievlaanderen/road-registry/compare/v1.15.0...v1.15.1) (2020-11-18)


### Bug Fixes

* remove set-env usage in gh-actions ([b20e3d0](https://github.com/informatievlaanderen/road-registry/commit/b20e3d0aa8487ace958ca3c32224fc2076bb672a))

# [1.15.0](https://github.com/informatievlaanderen/road-registry/compare/v1.14.0...v1.15.0) (2020-10-07)


### Features

* make atom feed processor more resilient ([#64](https://github.com/informatievlaanderen/road-registry/issues/64)) ([0ac8024](https://github.com/informatievlaanderen/road-registry/commit/0ac8024c5406cbf97cb2960e23736b0e4e0ad9e4))

# [1.14.0](https://github.com/informatievlaanderen/road-registry/compare/v1.13.0...v1.14.0) (2020-10-06)


### Features

* don't use basic auth for syndication feeds ([#63](https://github.com/informatievlaanderen/road-registry/issues/63)) ([a1d4537](https://github.com/informatievlaanderen/road-registry/commit/a1d453792ef125bfa2be4deff2530fe723c43eba))

# [1.13.0](https://github.com/informatievlaanderen/road-registry/compare/v1.12.2...v1.13.0) (2020-09-30)


### Bug Fixes

* move to 3.1.8 ([b7c58e9](https://github.com/informatievlaanderen/road-registry/commit/b7c58e95ee4f61c4d9699429fa1d79540a77439a))


### Features

* introduce modify and remove commands ([#51](https://github.com/informatievlaanderen/road-registry/issues/51)) ([36707c3](https://github.com/informatievlaanderen/road-registry/commit/36707c3665b75a7e9f82017da69b152b8b7fcdd2))
* introduce modify road node ([#50](https://github.com/informatievlaanderen/road-registry/issues/50)) ([2ad2015](https://github.com/informatievlaanderen/road-registry/commit/2ad2015964d61ce1089941cbc7cdd296285cb677))

## [1.12.2](https://github.com/informatievlaanderen/road-registry/compare/v1.12.1...v1.12.2) (2020-08-11)

## [1.12.1](https://github.com/informatievlaanderen/road-registry/compare/v1.12.0...v1.12.1) (2020-07-19)


### Bug Fixes

* move to 3.1.6 ([ce5cb7e](https://github.com/informatievlaanderen/road-registry/commit/ce5cb7ec488b1e8c97622a20b404a07a0caa0a87))

# [1.12.0](https://github.com/informatievlaanderen/road-registry/compare/v1.11.0...v1.12.0) (2020-07-02)


### Bug Fixes

* move to 3.1.5 ([7eebc0a](https://github.com/informatievlaanderen/road-registry/commit/7eebc0a43eda37204553f3dd8152568c6f4d1815))
* test ([#42](https://github.com/informatievlaanderen/road-registry/issues/42)) ([7c8185a](https://github.com/informatievlaanderen/road-registry/commit/7c8185a143cfa1220bd9bbdf0456b90be34ad3b4))
* the build ([#41](https://github.com/informatievlaanderen/road-registry/issues/41)) ([7c33e5b](https://github.com/informatievlaanderen/road-registry/commit/7c33e5b4e68822d34aa2e135b560605239d647aa))


### Features

* transaction id integration ([#39](https://github.com/informatievlaanderen/road-registry/issues/39)) ([97a2187](https://github.com/informatievlaanderen/road-registry/commit/97a2187d6fdc36a5cb70fc5970db9d45107d5c12))
* wms projection ([#29](https://github.com/informatievlaanderen/road-registry/issues/29)) ([fa73e36](https://github.com/informatievlaanderen/road-registry/commit/fa73e36f912e079ea66d750283d0b4658f517ea8))

# [1.11.0](https://github.com/informatievlaanderen/road-registry/compare/v1.10.0...v1.11.0) (2020-06-11)


### Features

* upgrade vbr api package ([703d7be](https://github.com/informatievlaanderen/road-registry/commit/703d7be749e85a0654ff9d871dba0d867cabb387))

# [1.10.0](https://github.com/informatievlaanderen/road-registry/compare/v1.9.1...v1.10.0) (2020-06-02)


### Features

* upgrade shaperon dependency ([#34](https://github.com/informatievlaanderen/road-registry/issues/34)) ([81a334e](https://github.com/informatievlaanderen/road-registry/commit/81a334e767f12e29c417bd24973aa8a202159972))

## [1.9.1](https://github.com/informatievlaanderen/road-registry/compare/v1.9.0...v1.9.1) (2020-05-23)


### Bug Fixes

* publish docker images ([355df7e](https://github.com/informatievlaanderen/road-registry/commit/355df7e74c04020d07927156401b0f43571c21d2))

# [1.9.0](https://github.com/informatievlaanderen/road-registry/compare/v1.8.0...v1.9.0) (2020-05-22)


### Bug Fixes

* force build ([bbb0b03](https://github.com/informatievlaanderen/road-registry/commit/bbb0b036d8dfd138e5886ca3765be1a8951c6f10))
* force gh actions ([660679b](https://github.com/informatievlaanderen/road-registry/commit/660679b0571c22dd12fccd3d83356f80f5fcae9f))


### Features

* move to 3.1.4 and gh actions ([25673a9](https://github.com/informatievlaanderen/road-registry/commit/25673a9babaef88bb34155e183db9de07ab14d6b))

# [1.8.0](https://github.com/informatievlaanderen/road-registry/compare/v1.7.0...v1.8.0) (2020-05-12)


### Features

* fix url slash and move api port ([#31](https://github.com/informatievlaanderen/road-registry/issues/31)) ([94034e8](https://github.com/informatievlaanderen/road-registry/commit/94034e8aba54ab63b8dee16845a9b0715adfe3cf))

# [1.7.0](https://github.com/informatievlaanderen/road-registry/compare/v1.6.0...v1.7.0) (2020-05-12)


### Features

* ui now deals with trailing slashes ([#30](https://github.com/informatievlaanderen/road-registry/issues/30)) ([42dc701](https://github.com/informatievlaanderen/road-registry/commit/42dc701f6d16165b8d1576cee06648269f10ceb0))

# [1.6.0](https://github.com/informatievlaanderen/road-registry/compare/v1.5.1...v1.6.0) (2020-05-12)


### Bug Fixes

* disable retryOnFailure ([#27](https://github.com/informatievlaanderen/road-registry/issues/27)) ([886b0c9](https://github.com/informatievlaanderen/road-registry/commit/886b0c922e157399585454de506a70641887e60e))
* remove leading slash for routes ([#25](https://github.com/informatievlaanderen/road-registry/issues/25)) ([afa5efa](https://github.com/informatievlaanderen/road-registry/commit/afa5efa39ec93c77ee8ef60a59f69bbf206fb70b))


### Features

* await IAsyncDisposable for streams in ZipArchiveWriters ([8c36f97](https://github.com/informatievlaanderen/road-registry/commit/8c36f973a312b1b00600b4f0094b4fe89f8f7abc))
* download product ([#23](https://github.com/informatievlaanderen/road-registry/issues/23)) ([01376c7](https://github.com/informatievlaanderen/road-registry/commit/01376c74d8cf3d93f09b619e14f2b15aa8bceadf))
* handle writing RoadNodes with an empty road network ([5453bf5](https://github.com/informatievlaanderen/road-registry/commit/5453bf5a3fb23ffa4ce0a2db932ba38a7aa5f72f))
* handle writing RoadNodes with an empty road network for product ([d09ecc2](https://github.com/informatievlaanderen/road-registry/commit/d09ecc2c5b63acd6f9d27be8d4c774156ab3037f))
* handle writing RoadSegments with an empty road network ([d579ee0](https://github.com/informatievlaanderen/road-registry/commit/d579ee0886944829ae090dd5b90858d6aff5727d))
* product download via api ([#18](https://github.com/informatievlaanderen/road-registry/issues/18)) ([0732ceb](https://github.com/informatievlaanderen/road-registry/commit/0732cebd120c607417632dc7171376150fbd40aa))


### Performance Improvements

* improve projection performance ([#28](https://github.com/informatievlaanderen/road-registry/issues/28)) ([7415ca0](https://github.com/informatievlaanderen/road-registry/commit/7415ca025b6cb3249dc7ed020edd2c8d80386f63))

## [1.5.1](https://github.com/informatievlaanderen/road-registry/compare/v1.5.0...v1.5.1) (2020-05-05)


### Bug Fixes

* docker composition ([#14](https://github.com/informatievlaanderen/road-registry/issues/14)) ([e11ac4b](https://github.com/informatievlaanderen/road-registry/commit/e11ac4bb183f47179f074bafd8189c0ce83c9b41))

# [1.5.0](https://github.com/informatievlaanderen/road-registry/compare/v1.4.1...v1.5.0) (2020-04-24)


### Features

* backoffice ui as nginx container ([6f965ba](https://github.com/informatievlaanderen/road-registry/commit/6f965ba30b76670eda798ced2728f465d76c100c))
* introduce distributed lock for several hosts ([df58a02](https://github.com/informatievlaanderen/road-registry/commit/df58a02ba43ea8b5d038b748f350e3cd7e836592))

## [1.4.1](https://github.com/informatievlaanderen/road-registry/compare/v1.4.0...v1.4.1) (2020-04-21)


### Bug Fixes

* reduced stream page size to 5000 ([#11](https://github.com/informatievlaanderen/road-registry/issues/11)) ([1447dd2](https://github.com/informatievlaanderen/road-registry/commit/1447dd2f4262a64fa75d86246c3b5afa26a18bf0))

# [1.4.0](https://github.com/informatievlaanderen/road-registry/compare/v1.3.1...v1.4.0) (2020-04-20)


### Features

* support seq for local development ([#10](https://github.com/informatievlaanderen/road-registry/issues/10)) ([1127bce](https://github.com/informatievlaanderen/road-registry/commit/1127bce3e8d1c6f880da825c463f4c3dbe811c94))

## [1.3.1](https://github.com/informatievlaanderen/road-registry/compare/v1.3.0...v1.3.1) (2020-04-12)


### Bug Fixes

* switch from getservice to getrequiredservice ([#9](https://github.com/informatievlaanderen/road-registry/issues/9)) ([8748e29](https://github.com/informatievlaanderen/road-registry/commit/8748e297f45fd1d449a31caf42de0f98ce476745))

# [1.3.0](https://github.com/informatievlaanderen/road-registry/compare/v1.2.0...v1.3.0) (2020-04-10)


### Features

* provision blob related resources ([#8](https://github.com/informatievlaanderen/road-registry/issues/8)) ([a5df84b](https://github.com/informatievlaanderen/road-registry/commit/a5df84b66e93c1f3c7e9bf1288d5b51610b4026d))

# [1.2.0](https://github.com/informatievlaanderen/road-registry/compare/v1.1.0...v1.2.0) (2020-04-09)


### Features

* program configuration logging improvement ([#7](https://github.com/informatievlaanderen/road-registry/issues/7)) ([e6b838b](https://github.com/informatievlaanderen/road-registry/commit/e6b838b47d7e2d768c5ae161407ada1c89cc58fe))

# [1.1.0](https://github.com/informatievlaanderen/road-registry/compare/v1.0.1...v1.1.0) (2020-04-08)


### Features

* decouple db init ([#5](https://github.com/informatievlaanderen/road-registry/issues/5)) ([bf109e3](https://github.com/informatievlaanderen/road-registry/commit/bf109e3a4e5ffc140718a4f2f084aa634e47a4e5))
* switch ident8 validation from hard coded list to structural ([#6](https://github.com/informatievlaanderen/road-registry/issues/6)) ([6cd87c4](https://github.com/informatievlaanderen/road-registry/commit/6cd87c46126dfdb2d803bdd4e390fc2cc8540e61))

## [1.0.1](https://github.com/informatievlaanderen/road-registry/compare/v1.0.0...v1.0.1) (2020-04-06)


### Bug Fixes

* use correct build user ([8202034](https://github.com/informatievlaanderen/road-registry/commit/820203484b525eb8db127e5689985ff9d2fb59f1))

# 1.0.0 (2020-04-06)


### Features

* dynamic api endpoint config discovery ([17be113](https://github.com/informatievlaanderen/road-registry/commit/17be113862c5b3711d947f111f3a99da5bcc7fe5))
* enhanced error reporting - removed custom AWS credential gathering ([e78f591](https://github.com/informatievlaanderen/road-registry/commit/e78f591b2bba5d4c988bb1ac6beac5551b4cace5))
* move legacy import and uploads to S3 - part 1 ([1d21223](https://github.com/informatievlaanderen/road-registry/commit/1d21223ea2f9c6cc6adfe238b234d65ff4c8f46f))
* move to netcore31 and move namespaces ([2d6cc0a](https://github.com/informatievlaanderen/road-registry/commit/2d6cc0a803ec11abad41df7fb8fbabd9af6f6163))
* show operator,reason,org and conditional link to archive ([b4318f7](https://github.com/informatievlaanderen/road-registry/commit/b4318f7888e9366d48cb5dd3deb07100c1f4dccb))
* startup resilience for extraction and loader ([2d15d01](https://github.com/informatievlaanderen/road-registry/commit/2d15d01b23c3575055b424c8646da61089e4771d))
* support change road network based on archive command ([58bc606](https://github.com/informatievlaanderen/road-registry/commit/58bc6063047cda591e9e3b20c3470a3041440c0f))
* support empty and filled legacy instances ([f939ff0](https://github.com/informatievlaanderen/road-registry/commit/f939ff04b57e2bb52464cfcd33063530a2097b4a))
