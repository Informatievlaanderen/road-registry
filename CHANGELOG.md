## [4.22.1](https://github.com/informatievlaanderen/road-registry/compare/v4.22.0...v4.22.1) (2025-02-07)


### Bug Fixes

* pipeline ([48e7982](https://github.com/informatievlaanderen/road-registry/commit/48e798246f5d46bca3d08ab37b3bb3522a2cf303))

# [4.22.0](https://github.com/informatievlaanderen/road-registry/compare/v4.21.7...v4.22.0) (2025-02-07)


### Bug Fixes

* pipeline ([f2de261](https://github.com/informatievlaanderen/road-registry/commit/f2de26118cf017f4d53514acc7d271aaa38534dc))


### Features

* bulk relink roadsegment streetname GAWR-6752 ([377afff](https://github.com/informatievlaanderen/road-registry/commit/377affffc466c31a6928ca3382c68ec2c13aeb47))

## [4.21.7](https://github.com/informatievlaanderen/road-registry/compare/v4.21.6...v4.21.7) (2025-02-03)


### Bug Fixes

* translation for OutlinedRoadSegmentRemoved ([3b6f459](https://github.com/informatievlaanderen/road-registry/commit/3b6f45908f28c8a4df8ddd1f4b1d166890f74b9d))

## [4.21.6](https://github.com/informatievlaanderen/road-registry/compare/v4.21.5...v4.21.6) (2025-02-03)


### Bug Fixes

* **ci:** update pipelines ([fcda88e](https://github.com/informatievlaanderen/road-registry/commit/fcda88e77243a47f457763f77bbe273e6163eae5))
* **feature-compare:** validate missing lanes/surfaces/widths when the dbf file is empty ([4c1a94c](https://github.com/informatievlaanderen/road-registry/commit/4c1a94cb556eb1d05a165725c279125b4cfeb4fe))

## [4.21.5](https://github.com/informatievlaanderen/road-registry/compare/v4.21.4...v4.21.5) (2025-01-31)

## [4.21.4](https://github.com/informatievlaanderen/road-registry/compare/v4.21.3...v4.21.4) (2025-01-30)


### Bug Fixes

* **wms:** only register overlapping transactionzones which have area of 0 (rounded) ([40d23e1](https://github.com/informatievlaanderen/road-registry/commit/40d23e13acfe1b1b062046deca3f22152f2db440))

## [4.21.3](https://github.com/informatievlaanderen/road-registry/compare/v4.21.2...v4.21.3) (2025-01-29)


### Bug Fixes

* feature-compare to prioritise current roadsegment record id instead of taking first available ([a7e13e2](https://github.com/informatievlaanderen/road-registry/commit/a7e13e2f3d9e324d3f6917abf869745899448acf))
* overlappingtransactionzones only those with area > 0 ([cbfbb4b](https://github.com/informatievlaanderen/road-registry/commit/cbfbb4ba5d62f6eb0115089e70c1fea356e5dbd2))

## [4.21.2](https://github.com/informatievlaanderen/road-registry/compare/v4.21.1...v4.21.2) (2025-01-24)


### Bug Fixes

* only add change files when extract request is not informative ([e8cc75e](https://github.com/informatievlaanderen/road-registry/commit/e8cc75ea0bc6e814323133876c6c96b5efccf5ab))

## [4.21.1](https://github.com/informatievlaanderen/road-registry/compare/v4.21.0...v4.21.1) (2025-01-23)

# [4.21.0](https://github.com/informatievlaanderen/road-registry/compare/v4.20.4...v4.21.0) (2025-01-23)


### Features

* add change files to extract ([ffa20ae](https://github.com/informatievlaanderen/road-registry/commit/ffa20aef13eab00306ff1c4bd332bda38b2023a2))

## [4.20.4](https://github.com/informatievlaanderen/road-registry/compare/v4.20.3...v4.20.4) (2025-01-22)


### Bug Fixes

* use other overlay calculation logic for WMS overlapping transactionzones ([0edc8e0](https://github.com/informatievlaanderen/road-registry/commit/0edc8e0b3ed3c3655076264582501d6d8c866756))

## [4.20.3](https://github.com/informatievlaanderen/road-registry/compare/v4.20.2...v4.20.3) (2025-01-22)


### Bug Fixes

* register StreetNameRenamed event when handling fusie events ([d512b9c](https://github.com/informatievlaanderen/road-registry/commit/d512b9c8847dbb702a911494ee23dae4ee7162ec))

## [4.20.2](https://github.com/informatievlaanderen/road-registry/compare/v4.20.1...v4.20.2) (2025-01-21)


### Bug Fixes

* **wms:** add spatial indexes to transactionzones ([0a979d9](https://github.com/informatievlaanderen/road-registry/commit/0a979d980a6a659e4e476d6c6c09b89c12ce8781))

## [4.20.1](https://github.com/informatievlaanderen/road-registry/compare/v4.20.0...v4.20.1) (2025-01-21)


### Bug Fixes

* **wms:** transactionzones db schema ([1d86285](https://github.com/informatievlaanderen/road-registry/commit/1d8628524dc5ebbf8848085d5abf7c9c6f6f4c7b))

# [4.20.0](https://github.com/informatievlaanderen/road-registry/compare/v4.19.1...v4.20.0) (2025-01-21)


### Features

* GAWR-6711 add (overlapping) transactionzones projections for wms ([9e4b4fe](https://github.com/informatievlaanderen/road-registry/commit/9e4b4fe383cc8e9e9394f4eab5ba75af02553059))

## [4.19.1](https://github.com/informatievlaanderen/road-registry/compare/v4.19.0...v4.19.1) (2025-01-17)


### Bug Fixes

* add new endpoints to AcmIdm tests ([#1591](https://github.com/informatievlaanderen/road-registry/issues/1591)) ([2048a58](https://github.com/informatievlaanderen/road-registry/commit/2048a580321d6d08c0f320d4e42176f96c4ebe31))
* request extract by file validation ([0782ed1](https://github.com/informatievlaanderen/road-registry/commit/0782ed18536016e6f3b5add3d89314eeb0c1ac64))

# [4.19.0](https://github.com/informatievlaanderen/road-registry/compare/v4.18.2...v4.19.0) (2025-01-17)


### Features

* GAWR-5717 use public api endpoints to download/upload files ([#1590](https://github.com/informatievlaanderen/road-registry/issues/1590)) ([4be7893](https://github.com/informatievlaanderen/road-registry/commit/4be7893643ad279f5d6d911aa14ad5e23ec4d5c4))

## [4.18.2](https://github.com/informatievlaanderen/road-registry/compare/v4.18.1...v4.18.2) (2025-01-14)


### Bug Fixes

* get pre-signed url for extract download response ([048f8ab](https://github.com/informatievlaanderen/road-registry/commit/048f8aba67cb949e8347347ce5723f837dddf35c))

## [4.18.1](https://github.com/informatievlaanderen/road-registry/compare/v4.18.0...v4.18.1) (2025-01-14)


### Bug Fixes

* swagger ([cd7afe4](https://github.com/informatievlaanderen/road-registry/commit/cd7afe4bbae797c1ed1e1e192e5da3b77831a574))

# [4.18.0](https://github.com/informatievlaanderen/road-registry/compare/v4.17.1...v4.18.0) (2025-01-14)


### Features

* GAWR-5717 add pre-signed url endpoints to download files ([#1587](https://github.com/informatievlaanderen/road-registry/issues/1587)) ([7ada002](https://github.com/informatievlaanderen/road-registry/commit/7ada002a5b1cf5aa29a3594ed53e031e185d4cc3))

## [4.17.1](https://github.com/informatievlaanderen/road-registry/compare/v4.17.0...v4.17.1) (2025-01-10)


### Bug Fixes

* don't pass authorization header when uploading using presigned url ([#1586](https://github.com/informatievlaanderen/road-registry/issues/1586)) ([a2d9152](https://github.com/informatievlaanderen/road-registry/commit/a2d91525039731fac206bca7bf9ec283e66aae51))

# [4.17.0](https://github.com/informatievlaanderen/road-registry/compare/v4.16.7...v4.17.0) (2025-01-02)


### Features

* add cache introspection ([ed95cb7](https://github.com/informatievlaanderen/road-registry/commit/ed95cb74c87966637acfda862452e4b5921741bc))

## [4.16.7](https://github.com/informatievlaanderen/road-registry/compare/v4.16.6...v4.16.7) (2024-12-18)


### Bug Fixes

* move Organizations Authorize attributes to endpoint instead of controller ([328d9ed](https://github.com/informatievlaanderen/road-registry/commit/328d9eded90910166145abb3c377460814d1f28a))

## [4.16.6](https://github.com/informatievlaanderen/road-registry/compare/v4.16.5...v4.16.6) (2024-12-06)


### Bug Fixes

* use messagehandling OffsetOverride ([#1582](https://github.com/informatievlaanderen/road-registry/issues/1582)) ([673118f](https://github.com/informatievlaanderen/road-registry/commit/673118f6ab78113473107bb54d598165a0b479e9))

## [4.16.5](https://github.com/informatievlaanderen/road-registry/compare/v4.16.4...v4.16.5) (2024-12-06)


### Bug Fixes

* featurecompare dealing with duplicate numbered road attributes ([#1583](https://github.com/informatievlaanderen/road-registry/issues/1583)) ([f76e431](https://github.com/informatievlaanderen/road-registry/commit/f76e4313b8d4062267c7b839de2f8235b54ecbbe))

## [4.16.4](https://github.com/informatievlaanderen/road-registry/compare/v4.16.3...v4.16.4) (2024-11-27)


### Bug Fixes

* version bump ([#1581](https://github.com/informatievlaanderen/road-registry/issues/1581)) ([80cb3dd](https://github.com/informatievlaanderen/road-registry/commit/80cb3dd4a9a2569dbd24e0ccd25d60ebd8ae83c3))

## [4.16.3](https://github.com/informatievlaanderen/road-registry/compare/v4.16.2...v4.16.3) (2024-11-25)


### Bug Fixes

* command queue status query ([#1579](https://github.com/informatievlaanderen/road-registry/issues/1579)) ([ec886a5](https://github.com/informatievlaanderen/road-registry/commit/ec886a54749ffff2d5b17e2b01759b1358c1ac5b))

## [4.16.2](https://github.com/informatievlaanderen/road-registry/compare/v4.16.1...v4.16.2) (2024-11-13)


### Bug Fixes

* log lines in single-line ([#1578](https://github.com/informatievlaanderen/road-registry/issues/1578)) ([c94ca13](https://github.com/informatievlaanderen/road-registry/commit/c94ca13a69abdcf637f627cb3a0fd2a88fe88135))

## [4.16.1](https://github.com/informatievlaanderen/road-registry/compare/v4.16.0...v4.16.1) (2024-11-13)


### Bug Fixes

* OrganizationCommandQueue dependency in synchost ([#1577](https://github.com/informatievlaanderen/road-registry/issues/1577)) ([fbbd821](https://github.com/informatievlaanderen/road-registry/commit/fbbd821b3b82ec517f3d80ed84dc00ace5312fac))

# [4.16.0](https://github.com/informatievlaanderen/road-registry/compare/v4.15.3...v4.16.0) (2024-11-12)


### Features

* stop host with unhandled exceptions and seperate healthcheck/organization commands from RoadNetworkCommandModule ([#1576](https://github.com/informatievlaanderen/road-registry/issues/1576)) ([41aa484](https://github.com/informatievlaanderen/road-registry/commit/41aa4842186186080e2cd282035cde6523096a4c))

## [4.15.3](https://github.com/informatievlaanderen/road-registry/compare/v4.15.2...v4.15.3) (2024-11-07)


### Bug Fixes

* projector query ([#1575](https://github.com/informatievlaanderen/road-registry/issues/1575)) ([333314f](https://github.com/informatievlaanderen/road-registry/commit/333314faaa3aea71fd4b46abd1894eb4716e89d8))

## [4.15.2](https://github.com/informatievlaanderen/road-registry/compare/v4.15.1...v4.15.2) (2024-11-07)


### Bug Fixes

* streetnameeventconsumer register events for outlined roadsegments on the correct stream ([#1574](https://github.com/informatievlaanderen/road-registry/issues/1574)) ([7f50863](https://github.com/informatievlaanderen/road-registry/commit/7f508636dd4beb34fa150b68a5fb7f97ae3142fa))

## [4.15.1](https://github.com/informatievlaanderen/road-registry/compare/v4.15.0...v4.15.1) (2024-11-07)


### Bug Fixes

* commandhost processors position in projector ([#1573](https://github.com/informatievlaanderen/road-registry/issues/1573)) ([661420b](https://github.com/informatievlaanderen/road-registry/commit/661420bfba6f8434c8faa70fae7511c2368fc624))

# [4.15.0](https://github.com/informatievlaanderen/road-registry/compare/v4.14.0...v4.15.0) (2024-11-07)


### Bug Fixes

* use municipality consumer tables instead of editorcontext ([#1571](https://github.com/informatievlaanderen/road-registry/issues/1571)) ([347fdf0](https://github.com/informatievlaanderen/road-registry/commit/347fdf0c5fab70243a2234dfe2d0387f48bec8f5))


### Features

* streetname event consumer register RoadNetworkChangesAccepted event instead of ChangeRoadNetwork command ([#1572](https://github.com/informatievlaanderen/road-registry/issues/1572)) ([18890d8](https://github.com/informatievlaanderen/road-registry/commit/18890d8c15cfe8282cffe2a3b92e3863ec6a1c0d))

# [4.14.0](https://github.com/informatievlaanderen/road-registry/compare/v4.13.2...v4.14.0) (2024-11-05)


### Bug Fixes

* deploy pipeline run system healthcheck ([#1566](https://github.com/informatievlaanderen/road-registry/issues/1566)) ([69d0e7b](https://github.com/informatievlaanderen/road-registry/commit/69d0e7b0aac54a42259f855f6c3c25339e6a4955))
* deploy pipeline run system healthcheck ([#1567](https://github.com/informatievlaanderen/road-registry/issues/1567)) ([8eae7b0](https://github.com/informatievlaanderen/road-registry/commit/8eae7b095fc9f069c9b47ce7dec38160e3f05065))
* deploy pipeline run system healthcheck ([#1568](https://github.com/informatievlaanderen/road-registry/issues/1568)) ([72de073](https://github.com/informatievlaanderen/road-registry/commit/72de07304711593521fdc9e3febe538a40061d1a))
* deploy pipeline use secrets instead of vars ([#1569](https://github.com/informatievlaanderen/road-registry/issues/1569)) ([e35a05d](https://github.com/informatievlaanderen/road-registry/commit/e35a05d7e22a82c1c2573c0b8d09dd5c0458d3b1))


### Features

* add sync from municipality registry ([#1570](https://github.com/informatievlaanderen/road-registry/issues/1570)) ([c7c10ec](https://github.com/informatievlaanderen/road-registry/commit/c7c10ec973daf840264379897dad9adbbb325875))

## [4.13.2](https://github.com/informatievlaanderen/road-registry/compare/v4.13.1...v4.13.2) (2024-10-29)


### Bug Fixes

* GAWR-6651 close extract on RoadNetworkChangesAccepted and not when RoadNet… ([#1564](https://github.com/informatievlaanderen/road-registry/issues/1564)) ([4731b4c](https://github.com/informatievlaanderen/road-registry/commit/4731b4cc4a4010bd5edf98c3053f09df3142ab93))

## [4.13.1](https://github.com/informatievlaanderen/road-registry/compare/v4.13.0...v4.13.1) (2024-10-24)


### Bug Fixes

* **wms:** correct geolocation view ([d5ac41c](https://github.com/informatievlaanderen/road-registry/commit/d5ac41ca676d9be4992ea5aaf1bccaeae27e72f7))
* **wms:** fix geolocation view ([463eee7](https://github.com/informatievlaanderen/road-registry/commit/463eee7a82c489a6d15160b56b16af299f1377ef))

# [4.13.0](https://github.com/informatievlaanderen/road-registry/compare/v4.12.0...v4.13.0) (2024-10-23)


### Bug Fixes

* cleanup ([#1561](https://github.com/informatievlaanderen/road-registry/issues/1561)) ([7e58ce7](https://github.com/informatievlaanderen/road-registry/commit/7e58ce746dcf2716fa9dc6f24e8c49db3132c625))


### Features

* **wms:** add geolocation view ([baa247f](https://github.com/informatievlaanderen/road-registry/commit/baa247f2130237983b3f6710877e48e3d3b6c0e5))

# [4.12.0](https://github.com/informatievlaanderen/road-registry/compare/v4.11.1...v4.12.0) (2024-10-21)


### Features

* validate streetnameid existance when file upload ([#1560](https://github.com/informatievlaanderen/road-registry/issues/1560)) ([d686758](https://github.com/informatievlaanderen/road-registry/commit/d6867586127e5aec966167499d214dce6c494d88))

## [4.11.1](https://github.com/informatievlaanderen/road-registry/compare/v4.11.0...v4.11.1) (2024-10-18)


### Bug Fixes

* deploy pipeline dependency on new lambda_prd step ([#1558](https://github.com/informatievlaanderen/road-registry/issues/1558)) ([a60a132](https://github.com/informatievlaanderen/road-registry/commit/a60a132385177bf6fddaf24945d3225b1f48ee52))
* remove buffer option when requesting municipality extract ([#1559](https://github.com/informatievlaanderen/road-registry/issues/1559)) ([f828034](https://github.com/informatievlaanderen/road-registry/commit/f8280342a793f82e23cdade217803663b147de2f))

# [4.11.0](https://github.com/informatievlaanderen/road-registry/compare/v4.10.1...v4.11.0) (2024-10-16)


### Bug Fixes

* extract request event processor higher catchUpBatchSize ([#1557](https://github.com/informatievlaanderen/road-registry/issues/1557)) ([f384c43](https://github.com/informatievlaanderen/road-registry/commit/f384c433f23ba7860c1ed1ab2086ea8a9c5492a3))


### Features

* GAWR-5728 add KboNumber and IsMaintainer to Organization ([#1553](https://github.com/informatievlaanderen/road-registry/issues/1553)) ([322c048](https://github.com/informatievlaanderen/road-registry/commit/322c048c506f63ecb1496e29444b3c95aad8f584))

## [4.10.1](https://github.com/informatievlaanderen/road-registry/compare/v4.10.0...v4.10.1) (2024-10-14)


### Bug Fixes

* only apply changefeed filter when not empty ([#1556](https://github.com/informatievlaanderen/road-registry/issues/1556)) ([12e35f5](https://github.com/informatievlaanderen/road-registry/commit/12e35f506848e7fd3e12ec6aa5b9247acf8edd51))

# [4.10.0](https://github.com/informatievlaanderen/road-registry/compare/v4.9.12...v4.10.0) (2024-10-04)


### Features

* GAWR-5728 add Organization V2 projection + endpoint to mark organizations as maintainers ([#1555](https://github.com/informatievlaanderen/road-registry/issues/1555)) ([2893608](https://github.com/informatievlaanderen/road-registry/commit/289360866b193f0d8549f0d3edc9ff98682e5d2c))

## [4.9.12](https://github.com/informatievlaanderen/road-registry/compare/v4.9.11...v4.9.12) (2024-10-02)


### Bug Fixes

* handling StreetNameWasRejectedBecauseOfMunicipalityMerger when NewPersistentLocalIds empty ([#1552](https://github.com/informatievlaanderen/road-registry/issues/1552)) ([d1689fe](https://github.com/informatievlaanderen/road-registry/commit/d1689fef2b0f5cdf9a3f25a269748618de290a11))
* type in pipeline ([#1551](https://github.com/informatievlaanderen/road-registry/issues/1551)) ([02aef11](https://github.com/informatievlaanderen/road-registry/commit/02aef116de9e4053a7cda1de2d44e5b7a201f1f0))

## [4.9.11](https://github.com/informatievlaanderen/road-registry/compare/v4.9.10...v4.9.11) (2024-09-27)


### Bug Fixes

* increase system healthcheck timeout ([#1550](https://github.com/informatievlaanderen/road-registry/issues/1550)) ([9f60519](https://github.com/informatievlaanderen/road-registry/commit/9f60519967ba5b1777fd8cec3f8864e651d15f50))

## [4.9.10](https://github.com/informatievlaanderen/road-registry/compare/v4.9.9...v4.9.10) (2024-09-27)


### Bug Fixes

* add healthcheck to integration tests ([#1548](https://github.com/informatievlaanderen/road-registry/issues/1548)) ([1e14f4a](https://github.com/informatievlaanderen/road-registry/commit/1e14f4a0deb629e0456c20feb1bcb87aacd0db09))
* healthcheck via pipeline ([#1549](https://github.com/informatievlaanderen/road-registry/issues/1549)) ([95b4c7d](https://github.com/informatievlaanderen/road-registry/commit/95b4c7d96e634460d713b9436a171d505e266f99))

## [4.9.9](https://github.com/informatievlaanderen/road-registry/compare/v4.9.8...v4.9.9) (2024-09-27)


### Bug Fixes

* wfs/wms road segment projection deal with unknown roadsegments instead of error ([#1547](https://github.com/informatievlaanderen/road-registry/issues/1547)) ([5a8dacc](https://github.com/informatievlaanderen/road-registry/commit/5a8dacc0230865e41d578e10c96176a6f93adcba))

## [4.9.8](https://github.com/informatievlaanderen/road-registry/compare/v4.9.7...v4.9.8) (2024-09-24)


### Bug Fixes

* slack notification message in release pipeline ([#1546](https://github.com/informatievlaanderen/road-registry/issues/1546)) ([5e631e8](https://github.com/informatievlaanderen/road-registry/commit/5e631e85c4637986cbf6a59148654ea6e6a5a09c))

## [4.9.7](https://github.com/informatievlaanderen/road-registry/compare/v4.9.6...v4.9.7) (2024-09-24)


### Bug Fixes

* do not allow streetname ID 0 + treat null as -9 ([#1545](https://github.com/informatievlaanderen/road-registry/issues/1545)) ([ab5505a](https://github.com/informatievlaanderen/road-registry/commit/ab5505a2384624e8d202dfb053c4337c300623a9))

## [4.9.6](https://github.com/informatievlaanderen/road-registry/compare/v4.9.5...v4.9.6) (2024-09-23)


### Bug Fixes

* styling checkbox inside vl-alert ([#1543](https://github.com/informatievlaanderen/road-registry/issues/1543)) ([b91cb10](https://github.com/informatievlaanderen/road-registry/commit/b91cb1041532c5d76820374170e392da6284b2c7))
* validate RoadSegmentId in FC europeanroad/nationalroad/numberedroad ([#1541](https://github.com/informatievlaanderen/road-registry/issues/1541)) ([6ddee84](https://github.com/informatievlaanderen/road-registry/commit/6ddee846dc055192b3d68d1f78c95d9128d1c5e5))
* validation message when uploading 2nd time for extract ([#1542](https://github.com/informatievlaanderen/road-registry/issues/1542)) ([0ef3d8f](https://github.com/informatievlaanderen/road-registry/commit/0ef3d8f0d4f5ce61ef80265384f49fd30af2905a))

## [4.9.5](https://github.com/informatievlaanderen/road-registry/compare/v4.9.4...v4.9.5) (2024-09-23)


### Bug Fixes

* get transactionzones geojson via backoffice-api ([#1540](https://github.com/informatievlaanderen/road-registry/issues/1540)) ([9b94545](https://github.com/informatievlaanderen/road-registry/commit/9b945453c28aab06a4fbe6a827fa58517cdd2807))

## [4.9.4](https://github.com/informatievlaanderen/road-registry/compare/v4.9.3...v4.9.4) (2024-09-20)


### Bug Fixes

* ExtractRequestRecordProjection deal with missing data ([#1539](https://github.com/informatievlaanderen/road-registry/issues/1539)) ([0c915f6](https://github.com/informatievlaanderen/road-registry/commit/0c915f618b0d3a774c78bbeb74e8040ca60f474c))

## [4.9.3](https://github.com/informatievlaanderen/road-registry/compare/v4.9.2...v4.9.3) (2024-09-20)


### Bug Fixes

* healthcheck pipeline ([#1538](https://github.com/informatievlaanderen/road-registry/issues/1538)) ([6c01ced](https://github.com/informatievlaanderen/road-registry/commit/6c01cedeba9fab6b937307b0d6c9a7486d8a430f))

## [4.9.2](https://github.com/informatievlaanderen/road-registry/compare/v4.9.1...v4.9.2) (2024-09-20)


### Bug Fixes

* system healthcheck via pipeline ([#1537](https://github.com/informatievlaanderen/road-registry/issues/1537)) ([4046c35](https://github.com/informatievlaanderen/road-registry/commit/4046c35170b2c7db819df9c9c67fcabbef241f41))

## [4.9.1](https://github.com/informatievlaanderen/road-registry/compare/v4.9.0...v4.9.1) (2024-09-20)


### Bug Fixes

* system healthcheck + add to pipeline ([40cf1b0](https://github.com/informatievlaanderen/road-registry/commit/40cf1b0e0ff479faebd7a8d6f026821c5d93f25a))

# [4.9.0](https://github.com/informatievlaanderen/road-registry/compare/v4.8.5...v4.9.0) (2024-09-20)


### Features

* system healthcheck endpoint ([#1535](https://github.com/informatievlaanderen/road-registry/issues/1535)) ([38ce04d](https://github.com/informatievlaanderen/road-registry/commit/38ce04d9e7b1a0c8e0ddbefd563e2eb9f59c0649))

## [4.8.5](https://github.com/informatievlaanderen/road-registry/compare/v4.8.4...v4.8.5) (2024-09-18)


### Bug Fixes

* set catchupBatchSize to 1 for extract overlap projection ([#1534](https://github.com/informatievlaanderen/road-registry/issues/1534)) ([6e802fe](https://github.com/informatievlaanderen/road-registry/commit/6e802fef8570c625e4e438e55100fd861deb3b0c))

## [4.8.4](https://github.com/informatievlaanderen/road-registry/compare/v4.8.3...v4.8.4) (2024-09-17)


### Bug Fixes

* remove old staging from release pipeline ([#1533](https://github.com/informatievlaanderen/road-registry/issues/1533)) ([5db0eb7](https://github.com/informatievlaanderen/road-registry/commit/5db0eb78aee776803a525a572c2fb9f59d75a3dc))

## [4.8.3](https://github.com/informatievlaanderen/road-registry/compare/v4.8.2...v4.8.3) (2024-09-17)


### Bug Fixes

* extract overlap check to include local records ([#1532](https://github.com/informatievlaanderen/road-registry/issues/1532)) ([6cf5759](https://github.com/informatievlaanderen/road-registry/commit/6cf5759a6f8a7ea1c587fb9f6f937bfbbd853176))

## [4.8.2](https://github.com/informatievlaanderen/road-registry/compare/v4.8.1...v4.8.2) (2024-09-16)


### Bug Fixes

* huge memory usage for GradeSeparatedJunctionFeatureCompareTranslator ([#1531](https://github.com/informatievlaanderen/road-registry/issues/1531)) ([22f61dc](https://github.com/informatievlaanderen/road-registry/commit/22f61dcc5891c2e222c2d1bd18dd9c4f04a18af5))

## [4.8.1](https://github.com/informatievlaanderen/road-registry/compare/v4.8.0...v4.8.1) (2024-09-12)


### Bug Fixes

* close extract when upload received for GRB flow ([#1530](https://github.com/informatievlaanderen/road-registry/issues/1530)) ([304bc50](https://github.com/informatievlaanderen/road-registry/commit/304bc50ebdc84e071803195f97757f5f65f2948b))

# [4.8.0](https://github.com/informatievlaanderen/road-registry/compare/v4.7.0...v4.8.0) (2024-09-10)


### Features

* add overlap warning on activity feed ([#1529](https://github.com/informatievlaanderen/road-registry/issues/1529)) ([5fa68cc](https://github.com/informatievlaanderen/road-registry/commit/5fa68ccc89ddc12a21e532f9c3a5102ce277ebe7))

# [4.7.0](https://github.com/informatievlaanderen/road-registry/compare/v4.6.5...v4.7.0) (2024-09-09)


### Features

* extract overlap check when requesting a new extract ([#1528](https://github.com/informatievlaanderen/road-registry/issues/1528)) ([4261db6](https://github.com/informatievlaanderen/road-registry/commit/4261db6aacc852da9968d45de37f18a1189986f0))

## [4.6.5](https://github.com/informatievlaanderen/road-registry/compare/v4.6.4...v4.6.5) (2024-09-05)


### Bug Fixes

* streetname event consumer when no new ids were provided for merger ([a04f8e0](https://github.com/informatievlaanderen/road-registry/commit/a04f8e0aa9abc209adfd5ee86db0edf80532595c))

## [4.6.4](https://github.com/informatievlaanderen/road-registry/compare/v4.6.3...v4.6.4) (2024-09-04)


### Bug Fixes

* no error spam when organization registry is temporarily unavailable ([3ca3793](https://github.com/informatievlaanderen/road-registry/commit/3ca3793d50177b72a0bb119f1c7e18fcbae8ecd4))

## [4.6.3](https://github.com/informatievlaanderen/road-registry/compare/v4.6.2...v4.6.3) (2024-08-30)


### Bug Fixes

* add event consumer to projector + EF query include local ([fa275ff](https://github.com/informatievlaanderen/road-registry/commit/fa275ffc6974d790af4286878d9ed29a99307463))
* integration tests ([9fec0b2](https://github.com/informatievlaanderen/road-registry/commit/9fec0b2d4c2710b147833d442027806da9b14acb))

## [4.6.2](https://github.com/informatievlaanderen/road-registry/compare/v4.6.1...v4.6.2) (2024-08-13)


### Bug Fixes

* use interface in dependency ([#1524](https://github.com/informatievlaanderen/road-registry/issues/1524)) ([72dbe9f](https://github.com/informatievlaanderen/road-registry/commit/72dbe9f4668e9e468e33962fb12c932a1437554b))

## [4.6.1](https://github.com/informatievlaanderen/road-registry/compare/v4.6.0...v4.6.1) (2024-08-13)


### Bug Fixes

* validate ziparchive before trying to read extract description ([#1523](https://github.com/informatievlaanderen/road-registry/issues/1523)) ([28707d6](https://github.com/informatievlaanderen/road-registry/commit/28707d64d287c83e2ea72d134627894ecafdbc01))

# [4.6.0](https://github.com/informatievlaanderen/road-registry/compare/v4.5.0...v4.6.0) (2024-08-12)


### Features

* limit length of road segment geometry ([#1522](https://github.com/informatievlaanderen/road-registry/issues/1522)) ([4f13d7a](https://github.com/informatievlaanderen/road-registry/commit/4f13d7ac3cd7e4e03add0a3043dfe336aaa08471))

# [4.5.0](https://github.com/informatievlaanderen/road-registry/compare/v4.4.0...v4.5.0) (2024-08-05)


### Features

* blacklist ovocodes ([7d2d7de](https://github.com/informatievlaanderen/road-registry/commit/7d2d7deb32df06a4fda0abe40fb928677246e6ea))
* consume streetname events for municipality merger ([#1520](https://github.com/informatievlaanderen/road-registry/issues/1520)) ([02e213c](https://github.com/informatievlaanderen/road-registry/commit/02e213ce1e521cb3e1d8de3d8b34c3dedaf755b1))
* GAWR-6543 add validation when trying to change an upgraded category ([0221065](https://github.com/informatievlaanderen/road-registry/commit/0221065470b7377ec96e3e96b50662b37157dcfb))

# [4.4.0](https://github.com/informatievlaanderen/road-registry/compare/v4.3.0...v4.4.0) (2024-07-08)


### Features

* add integration version projections ([a542685](https://github.com/informatievlaanderen/road-registry/commit/a542685c7d9a5910a96e416e5781e37e1d8c1b0a))

# [4.3.0](https://github.com/informatievlaanderen/road-registry/compare/v4.2.1...v4.3.0) (2024-07-03)


### Bug Fixes

* unit tests ([#1518](https://github.com/informatievlaanderen/road-registry/issues/1518)) ([e436fbf](https://github.com/informatievlaanderen/road-registry/commit/e436fbf0582c337b4fc089064b5a281252991567))


### Features

* add new roadsegment categories ([64d9fb9](https://github.com/informatievlaanderen/road-registry/commit/64d9fb9c8321517c85cd22157a992e779e9853d7))

## [4.2.1](https://github.com/informatievlaanderen/road-registry/compare/v4.2.0...v4.2.1) (2024-07-03)


### Bug Fixes

* remove old staging env for pipelines ([#1516](https://github.com/informatievlaanderen/road-registry/issues/1516)) ([56fa27d](https://github.com/informatievlaanderen/road-registry/commit/56fa27d9af4977e82e4208e8ef6acf900431328c))

# [4.2.0](https://github.com/informatievlaanderen/road-registry/compare/v4.1.7...v4.2.0) (2024-07-02)


### Bug Fixes

* always use backoffice-api for transaction zone layers ([#1515](https://github.com/informatievlaanderen/road-registry/issues/1515)) ([1c74a50](https://github.com/informatievlaanderen/road-registry/commit/1c74a50013c47c0820f08d7b112c3c0ca78a7689))
* revert temporary logging ([#1513](https://github.com/informatievlaanderen/road-registry/issues/1513)) ([36b4839](https://github.com/informatievlaanderen/road-registry/commit/36b483967d8c60ee2c7914fd7f7c19d297e3275d))


### Features

* integration latest item projections ([220074e](https://github.com/informatievlaanderen/road-registry/commit/220074e48bb7be0607fc0de226424ea10407791a))

## [4.1.7](https://github.com/informatievlaanderen/road-registry/compare/v4.1.6...v4.1.7) (2024-06-21)


### Bug Fixes

* add debug info ([#1512](https://github.com/informatievlaanderen/road-registry/issues/1512)) ([fec701e](https://github.com/informatievlaanderen/road-registry/commit/fec701e05faefe4c7b44303fd936b1628691ee64))

## [4.1.6](https://github.com/informatievlaanderen/road-registry/compare/v4.1.5...v4.1.6) (2024-06-21)


### Bug Fixes

* add temporary logging ([#1511](https://github.com/informatievlaanderen/road-registry/issues/1511)) ([91f7c27](https://github.com/informatievlaanderen/road-registry/commit/91f7c27cfa6f26adedebcc0317e1c318ca7a7453))

## [4.1.5](https://github.com/informatievlaanderen/road-registry/compare/v4.1.4...v4.1.5) (2024-06-20)


### Bug Fixes

* change feed mark segments as modified when number attribute changes ([1fe1120](https://github.com/informatievlaanderen/road-registry/commit/1fe11201cf75cf0f4d84e7713d499560b40dd501))
* update roadsegment version after loading snapshot when playing road number attribute events ([399566d](https://github.com/informatievlaanderen/road-registry/commit/399566d081df3b03d79210dd49e2a45b341fd5e6))

## [4.1.4](https://github.com/informatievlaanderen/road-registry/compare/v4.1.3...v4.1.4) (2024-06-19)


### Bug Fixes

* **ci:** new lambda deploy tst+stg ([35a86f2](https://github.com/informatievlaanderen/road-registry/commit/35a86f2c71a62586e48a1ae7dd0ead20913af33c))
* GAWR-6462 bump roadsegment version when changing linked number attributes ([#1505](https://github.com/informatievlaanderen/road-registry/issues/1505)) ([018c57c](https://github.com/informatievlaanderen/road-registry/commit/018c57cfa3ecf4a43eb6bf821ffcbc2d71bcc4f3))

## [4.1.3](https://github.com/informatievlaanderen/road-registry/compare/v4.1.2...v4.1.3) (2024-06-17)


### Bug Fixes

* pipeline disable integration-projectionhost push images to stg ([#1508](https://github.com/informatievlaanderen/road-registry/issues/1508)) ([ae89996](https://github.com/informatievlaanderen/road-registry/commit/ae89996824bba6a505549873fee538dcaab6f18a))

## [4.1.2](https://github.com/informatievlaanderen/road-registry/compare/v4.1.1...v4.1.2) (2024-06-17)


### Bug Fixes

* version bump ([#1507](https://github.com/informatievlaanderen/road-registry/issues/1507)) ([6e54b65](https://github.com/informatievlaanderen/road-registry/commit/6e54b65eaa6a11f68839c064c9af498588ff88bd))

## [4.1.1](https://github.com/informatievlaanderen/road-registry/compare/v4.1.0...v4.1.1) (2024-06-14)


### Bug Fixes

* remove query filter in integration projections ([#1504](https://github.com/informatievlaanderen/road-registry/issues/1504)) ([f2d8f9e](https://github.com/informatievlaanderen/road-registry/commit/f2d8f9efb47cfeb54fe8707d6de9166f665d6551))

# [4.1.0](https://github.com/informatievlaanderen/road-registry/compare/v4.0.20...v4.1.0) (2024-06-13)


### Features

* integration db projections ([5b591e3](https://github.com/informatievlaanderen/road-registry/commit/5b591e377a12c16197041c421de691def48585b6))

## [4.0.20](https://github.com/informatievlaanderen/road-registry/compare/v4.0.19...v4.0.20) (2024-05-29)


### Bug Fixes

* increase EditorContext commandtimeout ([#1496](https://github.com/informatievlaanderen/road-registry/issues/1496)) ([0e4b9d9](https://github.com/informatievlaanderen/road-registry/commit/0e4b9d9439985034ce2c6fd73ba7a00ff22a38b3))

## [4.0.19](https://github.com/informatievlaanderen/road-registry/compare/v4.0.18...v4.0.19) (2024-05-23)


### Bug Fixes

* **ci:** add new staging to deploy ([f4f243c](https://github.com/informatievlaanderen/road-registry/commit/f4f243c46e243c424867a5e94aa2b67be0713506))

## [4.0.18](https://github.com/informatievlaanderen/road-registry/compare/v4.0.17...v4.0.18) (2024-05-17)


### Bug Fixes

* use roadsegment id for error parameter ([#1492](https://github.com/informatievlaanderen/road-registry/issues/1492)) ([9d29cab](https://github.com/informatievlaanderen/road-registry/commit/9d29cabbdc13bff177c3864287bf2b404c3b0634))

## [4.0.17](https://github.com/informatievlaanderen/road-registry/compare/v4.0.16...v4.0.17) (2024-05-15)


### Bug Fixes

* add support for "Actual" parameter name in translations ([#1491](https://github.com/informatievlaanderen/road-registry/issues/1491)) ([835a2cd](https://github.com/informatievlaanderen/road-registry/commit/835a2cd7c5041417cc94ae77829d59e974cea84e))
* enable jobs processor deploy ([#1490](https://github.com/informatievlaanderen/road-registry/issues/1490)) ([ab323ef](https://github.com/informatievlaanderen/road-registry/commit/ab323ef03351fc644d77a31fa9f8c5cf40fab4ca))

## [4.0.16](https://github.com/informatievlaanderen/road-registry/compare/v4.0.15...v4.0.16) (2024-05-14)


### Bug Fixes

* show also outlined activity in the feed ([#1489](https://github.com/informatievlaanderen/road-registry/issues/1489)) ([e4c379b](https://github.com/informatievlaanderen/road-registry/commit/e4c379b6c2e33e2d437c5dae82e7010fcf9d8cc6))

## [4.0.15](https://github.com/informatievlaanderen/road-registry/compare/v4.0.14...v4.0.15) (2024-05-08)


### Bug Fixes

* GAWR-6423 add missing translation for RejectedChange ([#1486](https://github.com/informatievlaanderen/road-registry/issues/1486)) ([0f525bd](https://github.com/informatievlaanderen/road-registry/commit/0f525bd5f966a26b2b770ac130521b3f122d8535))

## [4.0.14](https://github.com/informatievlaanderen/road-registry/compare/v4.0.13...v4.0.14) (2024-05-07)


### Bug Fixes

* GAWR-6321 add WFS roadsegment indices ([349ac34](https://github.com/informatievlaanderen/road-registry/commit/349ac34469971c455eff0f9aaa64d870040d996b))

## [4.0.13](https://github.com/informatievlaanderen/road-registry/compare/v4.0.12...v4.0.13) (2024-05-06)


### Bug Fixes

* disable jobs processor deploy ([#1481](https://github.com/informatievlaanderen/road-registry/issues/1481)) ([e79f344](https://github.com/informatievlaanderen/road-registry/commit/e79f344e289c7aabd04eb5fbe60b07587344dff1))
* remove ValidationPipelineBehavior from snapshot lambda ([#1484](https://github.com/informatievlaanderen/road-registry/issues/1484)) ([08be79a](https://github.com/informatievlaanderen/road-registry/commit/08be79a9089e4fa52f24833586afe7d0a12064ad))

## [4.0.12](https://github.com/informatievlaanderen/road-registry/compare/v4.0.11...v4.0.12) (2024-04-22)


### Bug Fixes

* WR-941 edit roadsegment attributes dealing with existing duplicate attributes ([#1477](https://github.com/informatievlaanderen/road-registry/issues/1477)) ([184b81b](https://github.com/informatievlaanderen/road-registry/commit/184b81befb43f56f5607d54c7fefaf6b6d52b8c0))

## [4.0.11](https://github.com/informatievlaanderen/road-registry/compare/v4.0.10...v4.0.11) (2024-04-12)


### Bug Fixes

* WR-900 add support for binary/octet-stream ContentType ([#1471](https://github.com/informatievlaanderen/road-registry/issues/1471)) ([0832a18](https://github.com/informatievlaanderen/road-registry/commit/0832a1818600565618eb6e3fc1cc1011604af69a))

## [4.0.10](https://github.com/informatievlaanderen/road-registry/compare/v4.0.9...v4.0.10) (2024-04-12)


### Bug Fixes

* default Serilog formatter ([#1469](https://github.com/informatievlaanderen/road-registry/issues/1469)) ([7a1c1ac](https://github.com/informatievlaanderen/road-registry/commit/7a1c1ac22a5b324ced58e39ff538dce03ab7303b))
* WR-900 add invalid ContentType to error message ([#1470](https://github.com/informatievlaanderen/road-registry/issues/1470)) ([537902d](https://github.com/informatievlaanderen/road-registry/commit/537902dd357eed8434345426168ca5b12fc3215a))

## [4.0.9](https://github.com/informatievlaanderen/road-registry/compare/v4.0.8...v4.0.9) (2024-04-11)


### Bug Fixes

* WR-900 handle presignedupload response code ([#1468](https://github.com/informatievlaanderen/road-registry/issues/1468)) ([5218068](https://github.com/informatievlaanderen/road-registry/commit/52180681f728960670d0faa45c26d74f8bcd1c48))

## [4.0.8](https://github.com/informatievlaanderen/road-registry/compare/v4.0.7...v4.0.8) (2024-04-10)


### Bug Fixes

* update Be.Vlaanderen.Basisregisters.Api and enable authorization ([d688fd6](https://github.com/informatievlaanderen/road-registry/commit/d688fd6c451333087f91ac6cef136c53b03e7e4b))

## [4.0.7](https://github.com/informatievlaanderen/road-registry/compare/v4.0.6...v4.0.7) (2024-04-05)


### Bug Fixes

* WR-900 form parameter using presigned upload url ([#1464](https://github.com/informatievlaanderen/road-registry/issues/1464)) ([679eb64](https://github.com/informatievlaanderen/road-registry/commit/679eb64e06aeb89c1a66333aa81a33c4e4d68546))

## [4.0.6](https://github.com/informatievlaanderen/road-registry/compare/v4.0.5...v4.0.6) (2024-04-04)


### Bug Fixes

* add datadog dependencies ([c30426a](https://github.com/informatievlaanderen/road-registry/commit/c30426ac8a176e2d6a744eb3e86eaacbd823ac8a))
* pipeline remove huge unnecessary tools folder ([ec10e93](https://github.com/informatievlaanderen/road-registry/commit/ec10e938fa042f1d5396393cfccfc07910173af8))
* remove vbr datadog dependencies ([8753ef7](https://github.com/informatievlaanderen/road-registry/commit/8753ef719c5d8e427c9c417ce408ec70e7a22f8c))

## [4.0.5](https://github.com/informatievlaanderen/road-registry/compare/v4.0.4...v4.0.5) (2024-04-04)


### Bug Fixes

* disable jobs-processor in stg ([#1461](https://github.com/informatievlaanderen/road-registry/issues/1461)) ([2a0892b](https://github.com/informatievlaanderen/road-registry/commit/2a0892bb2e04adf135459e683960696d2c0fedc1))

## [4.0.4](https://github.com/informatievlaanderen/road-registry/compare/v4.0.3...v4.0.4) (2024-04-04)


### Bug Fixes

* enable jobs-processor in pipeline ([#1458](https://github.com/informatievlaanderen/road-registry/issues/1458)) ([4430cb5](https://github.com/informatievlaanderen/road-registry/commit/4430cb5ad1db154762744cceb9c55ae3e71fb98a))
* schedule cron to only run when TST env is active ([#1460](https://github.com/informatievlaanderen/road-registry/issues/1460)) ([bc5ade7](https://github.com/informatievlaanderen/road-registry/commit/bc5ade705cb6d10012477e8f859bec5edea5c3ca))
* use dotnet8 for schedule pipeline ([#1459](https://github.com/informatievlaanderen/road-registry/issues/1459)) ([0a0fbd3](https://github.com/informatievlaanderen/road-registry/commit/0a0fbd37fe3df06436ea1f78d39142185acaf417))

## [4.0.3](https://github.com/informatievlaanderen/road-registry/compare/v4.0.2...v4.0.3) (2024-03-27)


### Bug Fixes

* version in backoffice-ui docker ([#1452](https://github.com/informatievlaanderen/road-registry/issues/1452)) ([3acba8d](https://github.com/informatievlaanderen/road-registry/commit/3acba8d636a0e027d4aa14db58bdef3235edcbef))

## [4.0.2](https://github.com/informatievlaanderen/road-registry/compare/v4.0.1...v4.0.2) (2024-03-27)


### Bug Fixes

* add pipeline logging ([#1450](https://github.com/informatievlaanderen/road-registry/issues/1450)) ([039574d](https://github.com/informatievlaanderen/road-registry/commit/039574dd5260f912a65c7082d6a5b8c504955c28))
* pipeline logging ([#1451](https://github.com/informatievlaanderen/road-registry/issues/1451)) ([14cadbc](https://github.com/informatievlaanderen/road-registry/commit/14cadbc79615a80ae5b6a41e47a5d603b4188020))

## [4.0.1](https://github.com/informatievlaanderen/road-registry/compare/v4.0.0...v4.0.1) (2024-03-27)


### Bug Fixes

* add pipeline logging ([#1449](https://github.com/informatievlaanderen/road-registry/issues/1449)) ([d7f0f46](https://github.com/informatievlaanderen/road-registry/commit/d7f0f46fb5ab35c86ae18cf191b6238cca896e4a))

# [4.0.0](https://github.com/informatievlaanderen/road-registry/compare/v3.71.5...v4.0.0) (2024-03-26)


### Bug Fixes

* WR-900 pipeline disable jobs-process-upload ([#1439](https://github.com/informatievlaanderen/road-registry/issues/1439)) ([7cd3fa0](https://github.com/informatievlaanderen/road-registry/commit/7cd3fa0ef735cbdcc8036bdf3e21a4c20c6512d0))
* WR-966 add roadsegment identifiers to error messages ([#1440](https://github.com/informatievlaanderen/road-registry/issues/1440)) ([ee702c4](https://github.com/informatievlaanderen/road-registry/commit/ee702c4e2aa76e13db6b73500a043e26bb1ced4e))


### Features

* add v2 example ([ede55e9](https://github.com/informatievlaanderen/road-registry/commit/ede55e9e4d6e3e2698f5bc33a8971acd611387e5))
* move to dotnet 8.0.2 ([0fcb7a2](https://github.com/informatievlaanderen/road-registry/commit/0fcb7a25fb92e8b68bfeba449d28c98b4690f0dc))


### BREAKING CHANGES

* move to dotnet 8.0.2

## [3.71.5](https://github.com/informatievlaanderen/road-registry/compare/v3.71.4...v3.71.5) (2024-03-25)


### Bug Fixes

* release pipeline syntax ([#1438](https://github.com/informatievlaanderen/road-registry/issues/1438)) ([402defa](https://github.com/informatievlaanderen/road-registry/commit/402defac0c46c7901a90e9313633fe9c9b980ef7))
* WR-900 pipeline enable jobs-process-upload ([#1437](https://github.com/informatievlaanderen/road-registry/issues/1437)) ([f9c0bf7](https://github.com/informatievlaanderen/road-registry/commit/f9c0bf7181f80301fd772f1b8974fe68b0388b4c))

## [3.71.4](https://github.com/informatievlaanderen/road-registry/compare/v3.71.3...v3.71.4) (2024-03-22)


### Bug Fixes

* load TicketingOptions for system endpoints ([#1436](https://github.com/informatievlaanderen/road-registry/issues/1436)) ([fa63baa](https://github.com/informatievlaanderen/road-registry/commit/fa63baad01a52d1ba1708e1b7788e3b589434fb1))

## [3.71.3](https://github.com/informatievlaanderen/road-registry/compare/v3.71.2...v3.71.3) (2024-03-21)


### Bug Fixes

* always add record for RoadSegmentAdded with hard-delete roadsegmentprojections ([#1434](https://github.com/informatievlaanderen/road-registry/issues/1434)) ([03e3fdf](https://github.com/informatievlaanderen/road-registry/commit/03e3fdfff3385fa08a0bf6c5b08e418772141171))

## [3.71.2](https://github.com/informatievlaanderen/road-registry/compare/v3.71.1...v3.71.2) (2024-03-20)


### Bug Fixes

* WR-784 report original roadsegment id on upload when ID is reused for a modification ([#1432](https://github.com/informatievlaanderen/road-registry/issues/1432)) ([6a53f94](https://github.com/informatievlaanderen/road-registry/commit/6a53f94c19d2c8803a00d1a396aeab2b55a95a32))
* WR-941 change numberedroads detection when only ordinal/direction changes ([#1433](https://github.com/informatievlaanderen/road-registry/issues/1433)) ([6c68385](https://github.com/informatievlaanderen/road-registry/commit/6c68385ccfee685af7209a1c91c3e3914b9d498e))

## [3.71.1](https://github.com/informatievlaanderen/road-registry/compare/v3.71.0...v3.71.1) (2024-03-18)


### Bug Fixes

* move job Abstractions to BackOffice.Abstractions ([#1431](https://github.com/informatievlaanderen/road-registry/issues/1431)) ([c4d661a](https://github.com/informatievlaanderen/road-registry/commit/c4d661afad9a219d9af361f1cf5b51795b5e7a36))
* S3 healthchecks using disposed client ([#1430](https://github.com/informatievlaanderen/road-registry/issues/1430)) ([5630604](https://github.com/informatievlaanderen/road-registry/commit/56306040a0d2f88660ec03ce93b5803eef19d9c1))

# [3.71.0](https://github.com/informatievlaanderen/road-registry/compare/v3.70.7...v3.71.0) (2024-03-18)


### Bug Fixes

* pin workflow version in release pipeline ([#1428](https://github.com/informatievlaanderen/road-registry/issues/1428)) ([29d6d84](https://github.com/informatievlaanderen/road-registry/commit/29d6d8455e7ca51739ae1de134711669d1e420c9))
* service name job processor ([#1427](https://github.com/informatievlaanderen/road-registry/issues/1427)) ([79c4faa](https://github.com/informatievlaanderen/road-registry/commit/79c4faa21e8653b0c950915f4f59de0fedc6ad38))
* upload/download artifact to v3 ([#1429](https://github.com/informatievlaanderen/road-registry/issues/1429)) ([0e5d7a9](https://github.com/informatievlaanderen/road-registry/commit/0e5d7a9e4a8e957fc27c10197334b46fbf09e3d9))


### Features

* WR-900 add JobsProcessor to upload via public-api ([#1426](https://github.com/informatievlaanderen/road-registry/issues/1426)) ([db799b1](https://github.com/informatievlaanderen/road-registry/commit/db799b1dd1156e9fd0769be77bfbd5fbde29c5da))

## [3.70.7](https://github.com/informatievlaanderen/road-registry/compare/v3.70.6...v3.70.7) (2024-03-12)


### Bug Fixes

* WR-961 silently ignore removed roadsegments when correcting version ([#1425](https://github.com/informatievlaanderen/road-registry/issues/1425)) ([d9769d8](https://github.com/informatievlaanderen/road-registry/commit/d9769d81a8b4bd04e0cfd8e012849e2843642945))

## [3.70.6](https://github.com/informatievlaanderen/road-registry/compare/v3.70.5...v3.70.6) (2024-03-08)


### Bug Fixes

* WR-961 correct roadsegment versions load network from correct stream ([#1424](https://github.com/informatievlaanderen/road-registry/issues/1424)) ([95f1940](https://github.com/informatievlaanderen/road-registry/commit/95f1940a17544e229350d2315db4a7b41bc058d6))

## [3.70.5](https://github.com/informatievlaanderen/road-registry/compare/v3.70.4...v3.70.5) (2024-03-08)


### Bug Fixes

* WR-961 RoadSegmentVersion projection to be more flexible with test data ([#1423](https://github.com/informatievlaanderen/road-registry/issues/1423)) ([34ca5c7](https://github.com/informatievlaanderen/road-registry/commit/34ca5c78773a04fbfdc94ea722de8a74e9b0df89))

## [3.70.4](https://github.com/informatievlaanderen/road-registry/compare/v3.70.3...v3.70.4) (2024-03-08)


### Bug Fixes

* restore RoadNetworkExtractChangesArchiveFeatureCompareCompleted event ([#1421](https://github.com/informatievlaanderen/road-registry/issues/1421)) ([c3db9e6](https://github.com/informatievlaanderen/road-registry/commit/c3db9e67c7f392051eca37d05d76937cc25e2add))
* WR-961 keep version/geometryVersion when converting to measured ([#1422](https://github.com/informatievlaanderen/road-registry/issues/1422)) ([f074c07](https://github.com/informatievlaanderen/road-registry/commit/f074c07d7ee8d689a69b388571c82fccadaed886))

## [3.70.3](https://github.com/informatievlaanderen/road-registry/compare/v3.70.2...v3.70.3) (2024-03-06)


### Bug Fixes

* pipeline upgrade download/upload-artifact to v4 ([#1420](https://github.com/informatievlaanderen/road-registry/issues/1420)) ([eb37001](https://github.com/informatievlaanderen/road-registry/commit/eb370015684804f7070424ad5f0a9bd6d465833d))
* StreetNameEventConsumer query correct attribute tables ([#1419](https://github.com/informatievlaanderen/road-registry/issues/1419)) ([f034826](https://github.com/informatievlaanderen/road-registry/commit/f034826d201bf4cfc8a5e280c4b63ab563bb6c3b))

## [3.70.2](https://github.com/informatievlaanderen/road-registry/compare/v3.70.1...v3.70.2) (2024-03-05)


### Bug Fixes

* restore use of CustomSwaggerSchemaId instead of DataContract ([#1418](https://github.com/informatievlaanderen/road-registry/issues/1418)) ([933f6a8](https://github.com/informatievlaanderen/road-registry/commit/933f6a81cf51637e8146b742f8a56b57dee31e8b))

## [3.70.1](https://github.com/informatievlaanderen/road-registry/compare/v3.70.0...v3.70.1) (2024-03-05)


### Bug Fixes

* remove unused projects from release pipeline ([#1417](https://github.com/informatievlaanderen/road-registry/issues/1417)) ([8ed8e6e](https://github.com/informatievlaanderen/road-registry/commit/8ed8e6ee191bbd3063f422aa36628e226d38261c))

# [3.70.0](https://github.com/informatievlaanderen/road-registry/compare/v3.69.13...v3.70.0) (2024-03-05)


### Bug Fixes

* restore build script ([#1416](https://github.com/informatievlaanderen/road-registry/issues/1416)) ([73dc69a](https://github.com/informatievlaanderen/road-registry/commit/73dc69a91bef52576f491583a33d83b83bacc3a5))
* WR-954 OrganizationConsumer only change Organization when needed + deal with orgs with the same linked OVO-code ([#1413](https://github.com/informatievlaanderen/road-registry/issues/1413)) ([5746f0f](https://github.com/informatievlaanderen/road-registry/commit/5746f0f93cb8ba1dfe63ed7ff2f7a2db7a34bac6))


### Features

* WR-941 add European/national/numbered roads to roadsegment edit attributes and read endpoints ([#1415](https://github.com/informatievlaanderen/road-registry/issues/1415)) ([4d176a1](https://github.com/informatievlaanderen/road-registry/commit/4d176a16a9766a8fe17846613406bd5082842e97))
* WR-959 WR-922 convert RoadSegment V2 to V1 + remove after-FC logic and obsolete feature toggles ([#1411](https://github.com/informatievlaanderen/road-registry/issues/1411)) ([4cc478d](https://github.com/informatievlaanderen/road-registry/commit/4cc478d4277c756c8e86dae3fa91548d0cec99b0))

## [3.69.13](https://github.com/informatievlaanderen/road-registry/compare/v3.69.12...v3.69.13) (2024-03-04)


### Bug Fixes

* add logging ([#1412](https://github.com/informatievlaanderen/road-registry/issues/1412)) ([8ab3bdd](https://github.com/informatievlaanderen/road-registry/commit/8ab3bdd955ba591562e10f03c05d7b22f5022b2c))
* single deploy pipeline ([#1410](https://github.com/informatievlaanderen/road-registry/issues/1410)) ([f4b8874](https://github.com/informatievlaanderen/road-registry/commit/f4b8874d2d27000d74ade7776b9c1a73f0c3b4a7))

## [3.69.12](https://github.com/informatievlaanderen/road-registry/compare/v3.69.11...v3.69.12) (2024-02-28)


### Bug Fixes

* roadsegmentv2 product projection ([#1409](https://github.com/informatievlaanderen/road-registry/issues/1409)) ([92819dd](https://github.com/informatievlaanderen/road-registry/commit/92819dd524263fe284d54ea86ad1ce06bc0c9463))
* single deploy pipeline ([#1403](https://github.com/informatievlaanderen/road-registry/issues/1403)) ([05f6b23](https://github.com/informatievlaanderen/road-registry/commit/05f6b230f681db7ea2b4c4679acca38fcc14fe7c))
* single deploy pipeline ([#1404](https://github.com/informatievlaanderen/road-registry/issues/1404)) ([5550156](https://github.com/informatievlaanderen/road-registry/commit/5550156e4fbebd115fc115e8ca6252105ad1a406))
* single deploy pipeline ([#1405](https://github.com/informatievlaanderen/road-registry/issues/1405)) ([902131c](https://github.com/informatievlaanderen/road-registry/commit/902131ce03e559e28242389cae88b511f063ec59))
* single deploy pipeline ([#1406](https://github.com/informatievlaanderen/road-registry/issues/1406)) ([3c3b8bf](https://github.com/informatievlaanderen/road-registry/commit/3c3b8bf622e6c050624cabb1c0ae40ee5997e9e2))
* single deploy pipeline ([#1408](https://github.com/informatievlaanderen/road-registry/issues/1408)) ([7809e40](https://github.com/informatievlaanderen/road-registry/commit/7809e40dbb20ece74142471e41e4f2e734194981))
* single deploy pipeline default option as empty ([#1402](https://github.com/informatievlaanderen/road-registry/issues/1402)) ([aa55cba](https://github.com/informatievlaanderen/road-registry/commit/aa55cba12033e65d86f56512c56bc58cdf124496))

## [3.69.11](https://github.com/informatievlaanderen/road-registry/compare/v3.69.10...v3.69.11) (2024-02-28)


### Bug Fixes

* projection with null dbaserecord ([#1401](https://github.com/informatievlaanderen/road-registry/issues/1401)) ([32031e0](https://github.com/informatievlaanderen/road-registry/commit/32031e0bb1a0bc4058ea63e7e543340de0c3e271))

## [3.69.10](https://github.com/informatievlaanderen/road-registry/compare/v3.69.9...v3.69.10) (2024-02-28)


### Bug Fixes

* projection more flexible when RoadSegmentModified record doesn't exist ([#1400](https://github.com/informatievlaanderen/road-registry/issues/1400)) ([619f37d](https://github.com/informatievlaanderen/road-registry/commit/619f37da9b22c0fc0213b50f30247f3e307ce33f))

## [3.69.9](https://github.com/informatievlaanderen/road-registry/compare/v3.69.8...v3.69.9) (2024-02-28)


### Bug Fixes

* save context when skipping messages in OrganizationConsumer ([#1399](https://github.com/informatievlaanderen/road-registry/issues/1399)) ([1ae2f6f](https://github.com/informatievlaanderen/road-registry/commit/1ae2f6ffa5ff24585657810b0a9da52160384ad8))

## [3.69.8](https://github.com/informatievlaanderen/road-registry/compare/v3.69.7...v3.69.8) (2024-02-28)


### Bug Fixes

* WR-956 add RoadSegmentV2 for product; remove OrganizationRename handlers for editor/product RoadSegmentV1 projections ([#1398](https://github.com/informatievlaanderen/road-registry/issues/1398)) ([498b74e](https://github.com/informatievlaanderen/road-registry/commit/498b74ebd9830732f850e368b7a6fc5e92ecac44))

## [3.69.7](https://github.com/informatievlaanderen/road-registry/compare/v3.69.6...v3.69.7) (2024-02-27)


### Bug Fixes

* add logging in editor projection ([#1397](https://github.com/informatievlaanderen/road-registry/issues/1397)) ([0f29737](https://github.com/informatievlaanderen/road-registry/commit/0f297375245fde3916319b5a6dd0188fca64a3e8))

## [3.69.6](https://github.com/informatievlaanderen/road-registry/compare/v3.69.5...v3.69.6) (2024-02-26)


### Bug Fixes

* TST lambda deploy ([#1396](https://github.com/informatievlaanderen/road-registry/issues/1396)) ([405f810](https://github.com/informatievlaanderen/road-registry/commit/405f81055b99d2a6e822c5bb3095e1130115b1b7))

## [3.69.5](https://github.com/informatievlaanderen/road-registry/compare/v3.69.4...v3.69.5) (2024-02-26)


### Bug Fixes

* TST lambda deploy pipeline ([#1395](https://github.com/informatievlaanderen/road-registry/issues/1395)) ([5218c1a](https://github.com/informatievlaanderen/road-registry/commit/5218c1a25922f69baa86323a790b1212a3138e56))

## [3.69.4](https://github.com/informatievlaanderen/road-registry/compare/v3.69.3...v3.69.4) (2024-02-26)


### Bug Fixes

* TST lambda deploy ([#1394](https://github.com/informatievlaanderen/road-registry/issues/1394)) ([a32b759](https://github.com/informatievlaanderen/road-registry/commit/a32b759979a715e88f7e01aa07f32bed7a58df56))

## [3.69.3](https://github.com/informatievlaanderen/road-registry/compare/v3.69.2...v3.69.3) (2024-02-26)


### Bug Fixes

* TST lambda upload ([#1393](https://github.com/informatievlaanderen/road-registry/issues/1393)) ([3e1303e](https://github.com/informatievlaanderen/road-registry/commit/3e1303e41e6c617385827fdd7424eaa14e32b683))

## [3.69.2](https://github.com/informatievlaanderen/road-registry/compare/v3.69.1...v3.69.2) (2024-02-26)


### Bug Fixes

* TST deploy lambda's ([#1392](https://github.com/informatievlaanderen/road-registry/issues/1392)) ([278b471](https://github.com/informatievlaanderen/road-registry/commit/278b47169e2fc5bc9be7d314b7a85a1c0508a343))

## [3.69.1](https://github.com/informatievlaanderen/road-registry/compare/v3.69.0...v3.69.1) (2024-02-26)


### Bug Fixes

* extra logging when multiple organizations are found in cache with identical OVO-code ([#1391](https://github.com/informatievlaanderen/road-registry/issues/1391)) ([e8bed7b](https://github.com/informatievlaanderen/road-registry/commit/e8bed7b692160f1097266d25b99d63fc607eeb0e))
* validate input for code exchange endpoint ([#1389](https://github.com/informatievlaanderen/road-registry/issues/1389)) ([908acc9](https://github.com/informatievlaanderen/road-registry/commit/908acc96dbf2e1216646e09f358b3f1639ac544b))

# [3.69.0](https://github.com/informatievlaanderen/road-registry/compare/v3.68.0...v3.69.0) (2024-02-23)


### Bug Fixes

* release pipeline ([#1388](https://github.com/informatievlaanderen/road-registry/issues/1388)) ([740996d](https://github.com/informatievlaanderen/road-registry/commit/740996d70f3ca8999811411160e9bbbbc8a452b5))
* use new TST for release pipeline ([#1387](https://github.com/informatievlaanderen/road-registry/issues/1387)) ([f3e84d3](https://github.com/informatievlaanderen/road-registry/commit/f3e84d3f158b8dcae7f804279ff23c797d2519ef))
* WR-940 automatically redirect to activity page on successful upload ([#1386](https://github.com/informatievlaanderen/road-registry/issues/1386)) ([85094d5](https://github.com/informatievlaanderen/road-registry/commit/85094d5f128af48a020f93359c17e5b96abd2917))


### Features

* WR-940 add filter to ChangeFeed ([#1385](https://github.com/informatievlaanderen/road-registry/issues/1385)) ([b5cce77](https://github.com/informatievlaanderen/road-registry/commit/b5cce77f3b29001de1816be5fdd855cbd7c2c9ce))

# [3.68.0](https://github.com/informatievlaanderen/road-registry/compare/v3.67.17...v3.68.0) (2024-02-22)


### Bug Fixes

* deploy v2 pipeline for TST env ([#1383](https://github.com/informatievlaanderen/road-registry/issues/1383)) ([a8aec77](https://github.com/informatievlaanderen/road-registry/commit/a8aec77ff62220fc6a9e0dfe540e25f7e9475e6b))


### Features

* WR-928 handle StreetNameWasRenamed event ([#1384](https://github.com/informatievlaanderen/road-registry/issues/1384)) ([a692b5d](https://github.com/informatievlaanderen/road-registry/commit/a692b5d4ea45321948ff90c0aaa627eedc67464c))

## [3.67.17](https://github.com/informatievlaanderen/road-registry/compare/v3.67.16...v3.67.17) (2024-02-14)


### Bug Fixes

* **ci:** push images to devops ([1730127](https://github.com/informatievlaanderen/road-registry/commit/1730127f86fe5c5ddde4fc50d7dc94156b4f6ec8))

## [3.67.16](https://github.com/informatievlaanderen/road-registry/compare/v3.67.15...v3.67.16) (2024-02-14)


### Bug Fixes

* **ci:** add push images to devops ([ec807f0](https://github.com/informatievlaanderen/road-registry/commit/ec807f0c758be270a4303c5331e69d6af9284c66))

## [3.67.15](https://github.com/informatievlaanderen/road-registry/compare/v3.67.14...v3.67.15) (2024-02-08)


### Bug Fixes

* add deploy v2 pipeline ([#1380](https://github.com/informatievlaanderen/road-registry/issues/1380)) ([a76b496](https://github.com/informatievlaanderen/road-registry/commit/a76b496d68f05953d721fb68903546c2e076f440))
* deploy v2 pipeline ([#1381](https://github.com/informatievlaanderen/road-registry/issues/1381)) ([f00a252](https://github.com/informatievlaanderen/road-registry/commit/f00a252db4f3bbd178bb586a645032b8897098b3))
* geometry validation for outlined roadsegments ([#1382](https://github.com/informatievlaanderen/road-registry/issues/1382)) ([5c58364](https://github.com/informatievlaanderen/road-registry/commit/5c583645a8694ef73920edf91381fc03b2a747a9))
* polling-action version ([#1378](https://github.com/informatievlaanderen/road-registry/issues/1378)) ([c3855d2](https://github.com/informatievlaanderen/road-registry/commit/c3855d203374f470576d494490c33e3e3138ce02))
* polling-action version ([#1379](https://github.com/informatievlaanderen/road-registry/issues/1379)) ([b9d9819](https://github.com/informatievlaanderen/road-registry/commit/b9d981921d5afce69e86a97e06706c909d114196))
* random failing unit test due to outlined roadsegment rules ([#1377](https://github.com/informatievlaanderen/road-registry/issues/1377)) ([8116e01](https://github.com/informatievlaanderen/road-registry/commit/8116e01dac1b031e0ea322fd570f52edac7d23b5))

## [3.67.14](https://github.com/informatievlaanderen/road-registry/compare/v3.67.13...v3.67.14) (2024-02-05)


### Bug Fixes

* WR-933 unlink outlined roadsegments when streetname gets removed ([#1376](https://github.com/informatievlaanderen/road-registry/issues/1376)) ([c63a610](https://github.com/informatievlaanderen/road-registry/commit/c63a610f2dae6275075d4f88ebb3d3831467e1fd))

## [3.67.13](https://github.com/informatievlaanderen/road-registry/compare/v3.67.12...v3.67.13) (2024-02-01)


### Bug Fixes

* WR-936 singular record TOTPOS with value "0" should be filled in automatically ([#1375](https://github.com/informatievlaanderen/road-registry/issues/1375)) ([1037c24](https://github.com/informatievlaanderen/road-registry/commit/1037c24a2e33e6af9a33eb38508d92b4918be3d0))

## [3.67.12](https://github.com/informatievlaanderen/road-registry/compare/v3.67.11...v3.67.12) (2024-01-31)


### Bug Fixes

* WR-935 check for missing GradeSeparatedJunctions should ignore intersections with outlined RoadSegments ([#1374](https://github.com/informatievlaanderen/road-registry/issues/1374)) ([4044cb5](https://github.com/informatievlaanderen/road-registry/commit/4044cb523478fd96428fbcde5af53855efe3d773))

## [3.67.11](https://github.com/informatievlaanderen/road-registry/compare/v3.67.10...v3.67.11) (2024-01-31)


### Bug Fixes

* close extracts older than 6 months ([#1373](https://github.com/informatievlaanderen/road-registry/issues/1373)) ([c7bf0ef](https://github.com/informatievlaanderen/road-registry/commit/c7bf0ef478555b6eb7ab4e42fe8dea36dfef17f2))

## [3.67.10](https://github.com/informatievlaanderen/road-registry/compare/v3.67.9...v3.67.10) (2024-01-30)


### Bug Fixes

* WR-925 validation check when upload has segment with multiple lines or zip file contains duplicate files but with different casing ([#1372](https://github.com/informatievlaanderen/road-registry/issues/1372)) ([17bf92e](https://github.com/informatievlaanderen/road-registry/commit/17bf92e404c24428936c84e2d2101daf6d4a146f))

## [3.67.9](https://github.com/informatievlaanderen/road-registry/compare/v3.67.8...v3.67.9) (2024-01-30)


### Bug Fixes

* WR-934 pass ID when adding new EF entity right away ([#1371](https://github.com/informatievlaanderen/road-registry/issues/1371)) ([7d9af6c](https://github.com/informatievlaanderen/road-registry/commit/7d9af6cac003f700bccdb65f818c3a861294c043))

## [3.67.8](https://github.com/informatievlaanderen/road-registry/compare/v3.67.7...v3.67.8) (2024-01-26)


### Bug Fixes

* outlined segment rejections should show in feed ([#1369](https://github.com/informatievlaanderen/road-registry/issues/1369)) ([c9578eb](https://github.com/informatievlaanderen/road-registry/commit/c9578ebbc13b5724ef99d89046af4dadfb22d6d7))
* WR-932 convert Delete+Create to Update with soft-deleted objects in projections ([#1370](https://github.com/informatievlaanderen/road-registry/issues/1370)) ([16645a5](https://github.com/informatievlaanderen/road-registry/commit/16645a549e7ba8e3c6ad948aee07513e7ad6c03a))

## [3.67.7](https://github.com/informatievlaanderen/road-registry/compare/v3.67.6...v3.67.7) (2024-01-25)


### Bug Fixes

* add logging for overlap projection ([#1367](https://github.com/informatievlaanderen/road-registry/issues/1367)) ([91c35b3](https://github.com/informatievlaanderen/road-registry/commit/91c35b30e9e70409d52099551356de840119ecde))
* overlap logging ([#1368](https://github.com/informatievlaanderen/road-registry/issues/1368)) ([76b5842](https://github.com/informatievlaanderen/road-registry/commit/76b5842cd543acb6e686b3235a859bb0c7cdd0f5))

## [3.67.6](https://github.com/informatievlaanderen/road-registry/compare/v3.67.5...v3.67.6) (2024-01-25)


### Bug Fixes

* WR-929 migrate outlined from roadnetwork with no lanes/surfaces/widths ([#1366](https://github.com/informatievlaanderen/road-registry/issues/1366)) ([1228851](https://github.com/informatievlaanderen/road-registry/commit/1228851fb9a992ba276337e86649bc6c27a01f5b))

## [3.67.5](https://github.com/informatievlaanderen/road-registry/compare/v3.67.4...v3.67.5) (2024-01-24)


### Bug Fixes

* WR-929 add endpoint to migrate old outlined roadsegments to their own stream + LBLBEHEER fallback value for all situations ([#1365](https://github.com/informatievlaanderen/road-registry/issues/1365)) ([537201a](https://github.com/informatievlaanderen/road-registry/commit/537201aa40dba45fb797c3087bc23e375cf6d6c1))

## [3.67.4](https://github.com/informatievlaanderen/road-registry/compare/v3.67.3...v3.67.4) (2024-01-24)


### Bug Fixes

* remove obsolete featureflags ([#1364](https://github.com/informatievlaanderen/road-registry/issues/1364)) ([bb2cae1](https://github.com/informatievlaanderen/road-registry/commit/bb2cae15bb37d94ab12c4ff625102a6aec482f8c))

## [3.67.3](https://github.com/informatievlaanderen/road-registry/compare/v3.67.2...v3.67.3) (2024-01-24)


### Bug Fixes

* WR-909 StreetName sync in projector ([#1363](https://github.com/informatievlaanderen/road-registry/issues/1363)) ([2a419ae](https://github.com/informatievlaanderen/road-registry/commit/2a419ae23b42441cbf8a7bce7b1bee88c57bdfd9))

## [3.67.2](https://github.com/informatievlaanderen/road-registry/compare/v3.67.1...v3.67.2) (2024-01-24)


### Bug Fixes

* WR-923 add indexes ([#1362](https://github.com/informatievlaanderen/road-registry/issues/1362)) ([c571c9f](https://github.com/informatievlaanderen/road-registry/commit/c571c9f799c17a24d56e653a0fb1ad48fe4e6aef))

## [3.67.1](https://github.com/informatievlaanderen/road-registry/compare/v3.67.0...v3.67.1) (2024-01-24)


### Bug Fixes

* version bump ([#1360](https://github.com/informatievlaanderen/road-registry/issues/1360)) ([313d2af](https://github.com/informatievlaanderen/road-registry/commit/313d2af70a813225992e438c172d6f1c20a120b4))
* WR-923 StreetName projections ([#1361](https://github.com/informatievlaanderen/road-registry/issues/1361)) ([a4aca41](https://github.com/informatievlaanderen/road-registry/commit/a4aca41fa07e637fd8423063f77acaa781540a90))

# [3.67.0](https://github.com/informatievlaanderen/road-registry/compare/v3.66.6...v3.67.0) (2024-01-23)


### Bug Fixes

* WR-258 remove WR layer from map + rename url ([#1358](https://github.com/informatievlaanderen/road-registry/issues/1358)) ([a5f0ef1](https://github.com/informatievlaanderen/road-registry/commit/a5f0ef1e169ae14f6423c1a847ae8e2de7ca41fb))


### Features

* WR-923 WR-924 StreetName consumer produce internal events + unlink roadsegments upon streetname removal ([#1356](https://github.com/informatievlaanderen/road-registry/issues/1356)) ([a88df74](https://github.com/informatievlaanderen/road-registry/commit/a88df74a36b88007b6f7fc318f288e1094552181))

## [3.66.6](https://github.com/informatievlaanderen/road-registry/compare/v3.66.5...v3.66.6) (2024-01-23)

## [3.66.5](https://github.com/informatievlaanderen/road-registry/compare/v3.66.4...v3.66.5) (2024-01-09)


### Bug Fixes

* version bump ([#1354](https://github.com/informatievlaanderen/road-registry/issues/1354)) ([12a873c](https://github.com/informatievlaanderen/road-registry/commit/12a873cad4fc8e4a1e4552a1bb6b98375f927aeb))
* WR-913 remove usage of OwnsOne due to slow SQL being generated ([#1353](https://github.com/informatievlaanderen/road-registry/issues/1353)) ([5cde113](https://github.com/informatievlaanderen/road-registry/commit/5cde1137d50a8a434b25dd07dd024bbfa693c4cc))

## [3.66.4](https://github.com/informatievlaanderen/road-registry/compare/v3.66.3...v3.66.4) (2024-01-09)


### Bug Fixes

* WR-920 find old outlined roadsegments in default roadnetwork as fallback ([#1352](https://github.com/informatievlaanderen/road-registry/issues/1352)) ([69e93f6](https://github.com/informatievlaanderen/road-registry/commit/69e93f649714aee7d3ca6df457877d0deee9b9f5))

## [3.66.3](https://github.com/informatievlaanderen/road-registry/compare/v3.66.2...v3.66.3) (2024-01-09)


### Bug Fixes

* WR-919 allow MaintenanceAuthority of "-7" or "-8" in edit endpoints ([#1351](https://github.com/informatievlaanderen/road-registry/issues/1351)) ([6f62910](https://github.com/informatievlaanderen/road-registry/commit/6f62910eaa0fdc8f361ffd202a832ab25288f0d3))

## [3.66.2](https://github.com/informatievlaanderen/road-registry/compare/v3.66.1...v3.66.2) (2024-01-05)


### Bug Fixes

* add apikey to streetname-api calls ([#1350](https://github.com/informatievlaanderen/road-registry/issues/1350)) ([3cc78cb](https://github.com/informatievlaanderen/road-registry/commit/3cc78cb9fb5b5c143f261b968c9e03fe1e6f6498))
* error message with invalid roadsegment BEHEER value ([#1349](https://github.com/informatievlaanderen/road-registry/issues/1349)) ([bc2059c](https://github.com/informatievlaanderen/road-registry/commit/bc2059cda33125415ceaa5c2733624e1fff57362))

## [3.66.1](https://github.com/informatievlaanderen/road-registry/compare/v3.66.0...v3.66.1) (2024-01-05)


### Bug Fixes

* projector consumer endpoint response to be readable by public-api ([#1348](https://github.com/informatievlaanderen/road-registry/issues/1348)) ([ecee992](https://github.com/informatievlaanderen/road-registry/commit/ecee992420105cae02d6212139a9e7a9cbad8c19))
* WR-916 add featuretoggle for ProcessedMessages + OrganizationId must not contain whitespace ([#1347](https://github.com/informatievlaanderen/road-registry/issues/1347)) ([2bc61a6](https://github.com/informatievlaanderen/road-registry/commit/2bc61a6dbcf01a06d2b4aa120ccbf5a60117b086))

# [3.66.0](https://github.com/informatievlaanderen/road-registry/compare/v3.65.13...v3.66.0) (2024-01-05)


### Features

* WR-909 add backoffice processors and organization/streetname sync to projector api ([#1346](https://github.com/informatievlaanderen/road-registry/issues/1346)) ([4be4c90](https://github.com/informatievlaanderen/road-registry/commit/4be4c90d5ddae9b501da4fa465ecc0ed525b1697))

## [3.65.13](https://github.com/informatievlaanderen/road-registry/compare/v3.65.12...v3.65.13) (2024-01-03)


### Bug Fixes

* show activity in changefeed when download timeout occurred ([#1345](https://github.com/informatievlaanderen/road-registry/issues/1345)) ([d3a9c92](https://github.com/informatievlaanderen/road-registry/commit/d3a9c92fb43d8a09e9d31fc24209a6c4053446ed))
* WR-796 overlap record uniqueness ([#1344](https://github.com/informatievlaanderen/road-registry/issues/1344)) ([5ded26b](https://github.com/informatievlaanderen/road-registry/commit/5ded26becd0333fca5b564c5cd1e05a3e9136178))

## [3.65.12](https://github.com/informatievlaanderen/road-registry/compare/v3.65.11...v3.65.12) (2024-01-03)


### Bug Fixes

* allow MaintenanceAuthority of "-7" or "-8" in FeatureCompare ([#1343](https://github.com/informatievlaanderen/road-registry/issues/1343)) ([dea2507](https://github.com/informatievlaanderen/road-registry/commit/dea2507b1984dd0732f74fec981075e4dbbf156d))
* running ACM/IDM integration tests with api integration tests ([#1342](https://github.com/informatievlaanderen/road-registry/issues/1342)) ([ba98001](https://github.com/informatievlaanderen/road-registry/commit/ba98001758559ecb07bfd84e5671d7c8e0de7f90))
* WR-796 add projection for overlapping extracts ([#1341](https://github.com/informatievlaanderen/road-registry/issues/1341)) ([90ecb7c](https://github.com/informatievlaanderen/road-registry/commit/90ecb7c2aee505539a9992139a2d492e7875ed78))

## [3.65.11](https://github.com/informatievlaanderen/road-registry/compare/v3.65.10...v3.65.11) (2023-12-22)


### Bug Fixes

* WR-271 missing identifier for translations ([#1340](https://github.com/informatievlaanderen/road-registry/issues/1340)) ([a14c0e7](https://github.com/informatievlaanderen/road-registry/commit/a14c0e710ee4cb3c64bc50f39361641b357c05b6))
* WR-796 add projection for overlapping extracts ([#1337](https://github.com/informatievlaanderen/road-registry/issues/1337)) ([f2b7d7b](https://github.com/informatievlaanderen/road-registry/commit/f2b7d7b828dbe0e3f9c642078d8718982a63af10))

## [3.65.10](https://github.com/informatievlaanderen/road-registry/compare/v3.65.9...v3.65.10) (2023-12-22)


### Bug Fixes

* parameter IdentifierField in translation is optional ([#1339](https://github.com/informatievlaanderen/road-registry/issues/1339)) ([756bbd9](https://github.com/informatievlaanderen/road-registry/commit/756bbd96c40456d3f858f84052773b20c5325057))

## [3.65.9](https://github.com/informatievlaanderen/road-registry/compare/v3.65.8...v3.65.9) (2023-12-21)


### Bug Fixes

* trying to download an archive which timed out during creation ([#1338](https://github.com/informatievlaanderen/road-registry/issues/1338)) ([d3cad84](https://github.com/informatievlaanderen/road-registry/commit/d3cad84ab364006e3d9440092a20533c96230807))

## [3.65.8](https://github.com/informatievlaanderen/road-registry/compare/v3.65.7...v3.65.8) (2023-12-21)


### Bug Fixes

* WR-258 styling activity page ([#1336](https://github.com/informatievlaanderen/road-registry/issues/1336)) ([a0cedd5](https://github.com/informatievlaanderen/road-registry/commit/a0cedd50ee5826b4c23cf37011b3cd5c644e8b46))

## [3.65.7](https://github.com/informatievlaanderen/road-registry/compare/v3.65.6...v3.65.7) (2023-12-20)


### Bug Fixes

* WR-258 transactionzones in header only when authenticated ([#1335](https://github.com/informatievlaanderen/road-registry/issues/1335)) ([9a8db9b](https://github.com/informatievlaanderen/road-registry/commit/9a8db9b7d1e33ba4e7ef83beae430e5304a0ea71))

## [3.65.6](https://github.com/informatievlaanderen/road-registry/compare/v3.65.5...v3.65.6) (2023-12-20)


### Bug Fixes

* WR-258 transactionzones map ([#1334](https://github.com/informatievlaanderen/road-registry/issues/1334)) ([10f2261](https://github.com/informatievlaanderen/road-registry/commit/10f226190e2fa9bcc82c4e49cd47ad97284f6c8f))

## [3.65.5](https://github.com/informatievlaanderen/road-registry/compare/v3.65.4...v3.65.5) (2023-12-20)


### Bug Fixes

* WR-271 add more record info to Dutch error messages + re-enable ziparchivecleaner for standard upload ([#1333](https://github.com/informatievlaanderen/road-registry/issues/1333)) ([d6fec36](https://github.com/informatievlaanderen/road-registry/commit/d6fec36e23878e04e8ae4651911635daf8efb345))

## [3.65.4](https://github.com/informatievlaanderen/road-registry/compare/v3.65.3...v3.65.4) (2023-12-18)


### Bug Fixes

* Dutch translation ([#1332](https://github.com/informatievlaanderen/road-registry/issues/1332)) ([842e6e6](https://github.com/informatievlaanderen/road-registry/commit/842e6e606b867c3b46c1f52b11bce266d422ed9d))

## [3.65.3](https://github.com/informatievlaanderen/road-registry/compare/v3.65.2...v3.65.3) (2023-12-18)


### Bug Fixes

* WR-797 map MaintenanceAuthority to internal OrgId in FC before processing the feature ([#1330](https://github.com/informatievlaanderen/road-registry/issues/1330)) ([5e2202b](https://github.com/informatievlaanderen/road-registry/commit/5e2202b266459ede0cdacbb251f2fc38b17da82b))
* WR-908 downloadtimeoutoccurred for extracts with previously download available ([#1329](https://github.com/informatievlaanderen/road-registry/issues/1329)) ([e5f4cd9](https://github.com/informatievlaanderen/road-registry/commit/e5f4cd9b13e85a02623db6eb401ac608484b0c0c))

## [3.65.2](https://github.com/informatievlaanderen/road-registry/compare/v3.65.1...v3.65.2) (2023-12-12)


### Bug Fixes

* WR-797 extracthost dependencies for IOrganizationRepository ([#1328](https://github.com/informatievlaanderen/road-registry/issues/1328)) ([83edd1d](https://github.com/informatievlaanderen/road-registry/commit/83edd1dea2429244ee8dd6c6e066aa44f7bcdc75))

## [3.65.1](https://github.com/informatievlaanderen/road-registry/compare/v3.65.0...v3.65.1) (2023-12-12)


### Bug Fixes

* WR-905 only filter streams for changefeed on final accepted/rejected event ([#1326](https://github.com/informatievlaanderen/road-registry/issues/1326)) ([3802fac](https://github.com/informatievlaanderen/road-registry/commit/3802fac1ce4ffa8f432a87699dce43dac104008d))
* WR-906 remove streetname atom feed from statuspage ([#1327](https://github.com/informatievlaanderen/road-registry/issues/1327)) ([698c7c0](https://github.com/informatievlaanderen/road-registry/commit/698c7c09ef9632f47fb37827d728b5ae98f9c167))

# [3.65.0](https://github.com/informatievlaanderen/road-registry/compare/v3.64.0...v3.65.0) (2023-12-11)


### Bug Fixes

* WR-903 use same tolerance for all geometry/measurement related checks ([#1325](https://github.com/informatievlaanderen/road-registry/issues/1325)) ([d93377c](https://github.com/informatievlaanderen/road-registry/commit/d93377c6e2b6ac0c4f08197fad552b8aa9cb1ad6))


### Features

* add integrationdb ksql scripts ([#1317](https://github.com/informatievlaanderen/road-registry/issues/1317)) ([a043ba4](https://github.com/informatievlaanderen/road-registry/commit/a043ba4fc9f7b22cf0c16cde955473e08df40df3))

# [3.64.0](https://github.com/informatievlaanderen/road-registry/compare/v3.63.2...v3.64.0) (2023-12-11)


### Bug Fixes

* WR-887 don't send failure upload email when extract is missing/informative ([#1320](https://github.com/informatievlaanderen/road-registry/issues/1320)) ([50e1777](https://github.com/informatievlaanderen/road-registry/commit/50e17776438947cb2c927e7c6e64f188537b7be3))
* WR-902 roadnodes too close to each other can't both reuse the same id ([#1321](https://github.com/informatievlaanderen/road-registry/issues/1321)) ([b41aaa3](https://github.com/informatievlaanderen/road-registry/commit/b41aaa303b3a0ac296e883f60b19071bfaf4940a))


### Features

* WR-797 map MaintenanceAuthority as OVO-code to internal OrganizationId ([#1322](https://github.com/informatievlaanderen/road-registry/issues/1322)) ([b159871](https://github.com/informatievlaanderen/road-registry/commit/b159871b824bb34293ad55b47f1fe241848535e0))

## [3.63.2](https://github.com/informatievlaanderen/road-registry/compare/v3.63.1...v3.63.2) (2023-12-04)


### Bug Fixes

* ports for backoffice healthchecks ([#1318](https://github.com/informatievlaanderen/road-registry/issues/1318)) ([caa2a92](https://github.com/informatievlaanderen/road-registry/commit/caa2a92461d8234fae5091ed55431d1539fb5f4e))

## [3.63.1](https://github.com/informatievlaanderen/road-registry/compare/v3.63.0...v3.63.1) (2023-12-01)


### Bug Fixes

* expose ports for backoffice hosts ([#1316](https://github.com/informatievlaanderen/road-registry/issues/1316)) ([5c5055d](https://github.com/informatievlaanderen/road-registry/commit/5c5055de43e65002bf3d96a55fba331049fb1f6f))
* WR-896 keep menu items in UI when refreshing ([#1315](https://github.com/informatievlaanderen/road-registry/issues/1315)) ([fb7ca04](https://github.com/informatievlaanderen/road-registry/commit/fb7ca041f718eb7b20aef28069c75eefe8cc3dae))

# [3.63.0](https://github.com/informatievlaanderen/road-registry/compare/v3.62.11...v3.63.0) (2023-12-01)


### Features

* WR-886 use separate store aggregate for outlined roadsegments ([#1314](https://github.com/informatievlaanderen/road-registry/issues/1314)) ([dc18b90](https://github.com/informatievlaanderen/road-registry/commit/dc18b908f3907bf71f6158c4d1c1b755be068919))

## [3.62.11](https://github.com/informatievlaanderen/road-registry/compare/v3.62.10...v3.62.11) (2023-11-29)


### Bug Fixes

* WR-899 removing of duplicate GradeSeparatedJunctions ([#1313](https://github.com/informatievlaanderen/road-registry/issues/1313)) ([c9f8b6b](https://github.com/informatievlaanderen/road-registry/commit/c9f8b6be8f8bbeeb07b423142de9e3e0155e5921))

## [3.62.10](https://github.com/informatievlaanderen/road-registry/compare/v3.62.9...v3.62.10) (2023-11-27)


### Bug Fixes

* extract failed upload should never return 400 ([#1310](https://github.com/informatievlaanderen/road-registry/issues/1310)) ([97592eb](https://github.com/informatievlaanderen/road-registry/commit/97592ebe3569667e88525e0d415c497c0d96fc6a))

## [3.62.9](https://github.com/informatievlaanderen/road-registry/compare/v3.62.8...v3.62.9) (2023-11-24)


### Bug Fixes

* let lambda continue when sqs-messages blob is not found ([#1306](https://github.com/informatievlaanderen/road-registry/issues/1306)) ([c64979f](https://github.com/informatievlaanderen/road-registry/commit/c64979fc9c8d5c2675e99def6547957c3ee0ccff))

## [3.62.8](https://github.com/informatievlaanderen/road-registry/compare/v3.62.7...v3.62.8) (2023-11-17)


### Bug Fixes

* exclude invalid path requests error notifications for docs ([#1298](https://github.com/informatievlaanderen/road-registry/issues/1298)) ([1ffccf0](https://github.com/informatievlaanderen/road-registry/commit/1ffccf070833a52e619c80e73f3d2ec334c24330))

## [3.62.7](https://github.com/informatievlaanderen/road-registry/compare/v3.62.6...v3.62.7) (2023-11-09)


### Bug Fixes

* WR-886 add logging to track timing of actions ([#1293](https://github.com/informatievlaanderen/road-registry/issues/1293)) ([4e17dcd](https://github.com/informatievlaanderen/road-registry/commit/4e17dcdb2a7c0d45943ac4e843a728ae19a131c4))

## [3.62.6](https://github.com/informatievlaanderen/road-registry/compare/v3.62.5...v3.62.6) (2023-11-09)


### Bug Fixes

* use timestamped filename for s3 healthcheck ([#1292](https://github.com/informatievlaanderen/road-registry/issues/1292)) ([d52743c](https://github.com/informatievlaanderen/road-registry/commit/d52743c8f86f6a2d27f04bb4d4bbd5f700d0ff5b))

## [3.62.5](https://github.com/informatievlaanderen/road-registry/compare/v3.62.4...v3.62.5) (2023-11-08)


### Bug Fixes

* extra healthcheck info when failing ([#1291](https://github.com/informatievlaanderen/road-registry/issues/1291)) ([27f0c0b](https://github.com/informatievlaanderen/road-registry/commit/27f0c0b7f4e18ab0a31287d222fc483bb90dfb75))
* remove healthchecks from lambda ([#1289](https://github.com/informatievlaanderen/road-registry/issues/1289)) ([e7b3cf6](https://github.com/informatievlaanderen/road-registry/commit/e7b3cf68a00beef7f5bab127d3a2945e983732e5))
* ticketing healthcheck delete ticket at the end ([#1290](https://github.com/informatievlaanderen/road-registry/issues/1290)) ([d038d1b](https://github.com/informatievlaanderen/road-registry/commit/d038d1b7a195b1e84c8f1f1f1b721b8fade7a085))

## [3.62.4](https://github.com/informatievlaanderen/road-registry/compare/v3.62.3...v3.62.4) (2023-11-08)


### Bug Fixes

* pipeline change newprd env to prd ([#1285](https://github.com/informatievlaanderen/road-registry/issues/1285)) ([edf2bac](https://github.com/informatievlaanderen/road-registry/commit/edf2bacaa9c8f8ce3988fda60e29da073d5c28a4))
* random integration test failures ([#1282](https://github.com/informatievlaanderen/road-registry/issues/1282)) ([705d42b](https://github.com/informatievlaanderen/road-registry/commit/705d42bee393180a5544eda59082435b506edb77))
* WR-754 retry-after calculation ([#1284](https://github.com/informatievlaanderen/road-registry/issues/1284)) ([f101a3f](https://github.com/informatievlaanderen/road-registry/commit/f101a3f50fe02b90ccc4f5e3ce006bd53b050821))
* WR-872 validate roadsegment/roadnode ids uniqueness across change and integration files ([#1286](https://github.com/informatievlaanderen/road-registry/issues/1286)) ([7f67494](https://github.com/informatievlaanderen/road-registry/commit/7f674948dfc19b969b8675c97c88dabdcd844c87))
* WR-884 validate national/european road records to be unique ([#1287](https://github.com/informatievlaanderen/road-registry/issues/1287)) ([5482105](https://github.com/informatievlaanderen/road-registry/commit/5482105f5414ddee01dd0f6bf7c4187c00848ce5))
* WR-885 format coordinates + don't show validation errors in Slack ([#1288](https://github.com/informatievlaanderen/road-registry/issues/1288)) ([0fd2f1f](https://github.com/informatievlaanderen/road-registry/commit/0fd2f1f6257a7fc3996ff044388be21c26a62fc9))

## [3.62.3](https://github.com/informatievlaanderen/road-registry/compare/v3.62.2...v3.62.3) (2023-11-06)


### Bug Fixes

* hard coded tag for deployment script + integration tests failure notification ([#1280](https://github.com/informatievlaanderen/road-registry/issues/1280)) ([1504cc9](https://github.com/informatievlaanderen/road-registry/commit/1504cc934ab76b15f584333759aad470bd96ef86))
* hard coded tag of deployment script ([#1281](https://github.com/informatievlaanderen/road-registry/issues/1281)) ([27e0506](https://github.com/informatievlaanderen/road-registry/commit/27e050650772db0f1f00c207f8aef451c8faee43))

## [3.62.2](https://github.com/informatievlaanderen/road-registry/compare/v3.62.1...v3.62.2) (2023-11-03)


### Bug Fixes

* WR-881 UI hide actions when user does not have rights ([#1279](https://github.com/informatievlaanderen/road-registry/issues/1279)) ([f2023c6](https://github.com/informatievlaanderen/road-registry/commit/f2023c6bd3bce26ba2e7555c257e120f3cdb4fde))

## [3.62.1](https://github.com/informatievlaanderen/road-registry/compare/v3.62.0...v3.62.1) (2023-10-27)


### Bug Fixes

* release failure notification ([#1272](https://github.com/informatievlaanderen/road-registry/issues/1272)) ([87b46ee](https://github.com/informatievlaanderen/road-registry/commit/87b46eef9966780c3fd8951f83b0c96fa03db623))
* WR-754 restart timer before processing record ([#1271](https://github.com/informatievlaanderen/road-registry/issues/1271)) ([903e87a](https://github.com/informatievlaanderen/road-registry/commit/903e87acf09d196128c36e84bc298015209c62c5))

# [3.62.0](https://github.com/informatievlaanderen/road-registry/compare/v3.61.5...v3.62.0) (2023-10-27)


### Bug Fixes

* WR-752 do not update BEGINORG/LBLBEGINORG on modification/removal ([#1270](https://github.com/informatievlaanderen/road-registry/issues/1270)) ([83c3c16](https://github.com/informatievlaanderen/road-registry/commit/83c3c162b827318f77b318bc7487842d2baf954f))


### Features

* WR-844 add integration tests for ACM/IDM ([#1269](https://github.com/informatievlaanderen/road-registry/issues/1269)) ([43d4017](https://github.com/informatievlaanderen/road-registry/commit/43d4017d6b7262155ffec69fbd0b44b374fa640a))

## [3.61.5](https://github.com/informatievlaanderen/road-registry/compare/v3.61.4...v3.61.5) (2023-10-26)


### Bug Fixes

* fail release when release is skipped ([#1268](https://github.com/informatievlaanderen/road-registry/issues/1268)) ([b155e3b](https://github.com/informatievlaanderen/road-registry/commit/b155e3b046daa6123f5b1bdaeecf05cba29522e5))

## [3.61.4](https://github.com/informatievlaanderen/road-registry/compare/v3.61.3...v3.61.4) (2023-10-26)


### Bug Fixes

* pipeline notification when release is skipped ([#1267](https://github.com/informatievlaanderen/road-registry/issues/1267)) ([4804c44](https://github.com/informatievlaanderen/road-registry/commit/4804c4447d45843eeb900c8ebf964de6be4622fb))

## [3.61.3](https://github.com/informatievlaanderen/road-registry/compare/v3.61.2...v3.61.3) (2023-10-26)


### Bug Fixes

* move reusable workflow steps to actions folder ([#1263](https://github.com/informatievlaanderen/road-registry/issues/1263)) ([ed7cbf9](https://github.com/informatievlaanderen/road-registry/commit/ed7cbf93b13fd3421d7db97626acfb2944e12193))
* move reusable workflow steps to actions folder ([#1264](https://github.com/informatievlaanderen/road-registry/issues/1264)) ([ad01cc3](https://github.com/informatievlaanderen/road-registry/commit/ad01cc3d63e7a9855dbccd419d78e21ef7bf63b8))
* pipeline failure notifications ([#1262](https://github.com/informatievlaanderen/road-registry/issues/1262)) ([951fc03](https://github.com/informatievlaanderen/road-registry/commit/951fc0343f71a81e5aa0f1d68036ba9ce667cd8e))
* pipeline move actions back to workflows ([#1266](https://github.com/informatievlaanderen/road-registry/issues/1266)) ([d9e32e4](https://github.com/informatievlaanderen/road-registry/commit/d9e32e44664323677d40cd7d48fac9c2996cf6d9))
* release pipeline add notification when release is skipped ([#1265](https://github.com/informatievlaanderen/road-registry/issues/1265)) ([b42669e](https://github.com/informatievlaanderen/road-registry/commit/b42669e3f6b086044a8d79a94afdc28c2d1621ef))

## [3.61.2](https://github.com/informatievlaanderen/road-registry/compare/v3.61.1...v3.61.2) (2023-10-26)


### Bug Fixes

* remove acmidm healthcheck + add pipeline failure notification ([#1261](https://github.com/informatievlaanderen/road-registry/issues/1261)) ([e69018c](https://github.com/informatievlaanderen/road-registry/commit/e69018cbd35b7e3af22824367a2cab3afbbf0f4d))

## [3.61.1](https://github.com/informatievlaanderen/road-registry/compare/v3.61.0...v3.61.1) (2023-10-25)


### Bug Fixes

* lambda config binding of AcquireLockRetryDelaySeconds + change lambda AggregateIds ([#1259](https://github.com/informatievlaanderen/road-registry/issues/1259)) ([f4b7cef](https://github.com/informatievlaanderen/road-registry/commit/f4b7cefc2646d7eeb89f0094edbefe400205488b))

# [3.61.0](https://github.com/informatievlaanderen/road-registry/compare/v3.60.8...v3.61.0) (2023-10-25)


### Bug Fixes

* move CustomSchemaIds out of AddRoadRegistrySchemaFilters ([#1258](https://github.com/informatievlaanderen/road-registry/issues/1258)) ([3afd97e](https://github.com/informatievlaanderen/road-registry/commit/3afd97e20d01ab258097a909a09cc482d6864bf4))
* set AdminHost to AlwaysRunning for Development ([#1257](https://github.com/informatievlaanderen/road-registry/issues/1257)) ([eb512a5](https://github.com/informatievlaanderen/road-registry/commit/eb512a5aae72ae4d1380c101dc07d28d7c6010cb))


### Features

* WR-845 add healthcheck for hosted services their running status ([#1256](https://github.com/informatievlaanderen/road-registry/issues/1256)) ([fcd90d7](https://github.com/informatievlaanderen/road-registry/commit/fcd90d7e3b3c154fcb8c6437685dd30327e770a5))

## [3.60.8](https://github.com/informatievlaanderen/road-registry/compare/v3.60.7...v3.60.8) (2023-10-24)


### Bug Fixes

* WR-877 report original ID in activity feed when added roadnode/roadsegment is rejected ([#1255](https://github.com/informatievlaanderen/road-registry/issues/1255)) ([065ed27](https://github.com/informatievlaanderen/road-registry/commit/065ed27ca7ce86ef05c32826cdea20174c9a34c7))

## [3.60.7](https://github.com/informatievlaanderen/road-registry/compare/v3.60.6...v3.60.7) (2023-10-23)


### Bug Fixes

* WR-868 add handlers for organization renamings to projections ([#1249](https://github.com/informatievlaanderen/road-registry/issues/1249)) ([35442f2](https://github.com/informatievlaanderen/road-registry/commit/35442f295dfedba9c8a9e314f48bcdfe3bd78dcd))
* WR-868 remove default true for NameModified/OvoCodeModified ([#1250](https://github.com/informatievlaanderen/road-registry/issues/1250)) ([eec91cd](https://github.com/informatievlaanderen/road-registry/commit/eec91cdcad84eb46430013fe5cc8604aea38dabe))
* WR-873 validate missing integration roadsegment files on upload + ensure transactionzone upload always has exactly 1 record ([#1248](https://github.com/informatievlaanderen/road-registry/issues/1248)) ([a3db886](https://github.com/informatievlaanderen/road-registry/commit/a3db886312fc3a44069104f4f30623fe6c06ed3f))
* WR-875 missing descriptions in Swagger ([#1251](https://github.com/informatievlaanderen/road-registry/issues/1251)) ([9108443](https://github.com/informatievlaanderen/road-registry/commit/910844356668adb05857f8776cd6f11b6e5bd05e))

## [3.60.6](https://github.com/informatievlaanderen/road-registry/compare/v3.60.5...v3.60.6) (2023-10-19)


### Bug Fixes

* remove environment approval check for deploy pipelines ([#1246](https://github.com/informatievlaanderen/road-registry/issues/1246)) ([55154fe](https://github.com/informatievlaanderen/road-registry/commit/55154fe03da09702a27b4981f05695e6a4ae8869))
* roadsegments must always have a lane/surface/width attribute ([#1247](https://github.com/informatievlaanderen/road-registry/issues/1247)) ([1f82e41](https://github.com/informatievlaanderen/road-registry/commit/1f82e417c8eb67a9782f0c06df8a397f7011b9f7))

## [3.60.5](https://github.com/informatievlaanderen/road-registry/compare/v3.60.4...v3.60.5) (2023-10-18)


### Bug Fixes

* write TransactionZone shape only with XY ordinates (without ZM) ([#1245](https://github.com/informatievlaanderen/road-registry/issues/1245)) ([fb90471](https://github.com/informatievlaanderen/road-registry/commit/fb9047154a4f100c9be82a6b728d91ddf4e34c05))

## [3.60.4](https://github.com/informatievlaanderen/road-registry/compare/v3.60.3...v3.60.4) (2023-10-17)


### Bug Fixes

* missing lanes/surfaces/widths when link/unlinking streetname from roadsegment ([#1244](https://github.com/informatievlaanderen/road-registry/issues/1244)) ([e757d57](https://github.com/informatievlaanderen/road-registry/commit/e757d57b88056d7843cb1fd81db67385c95447c7))

## [3.60.3](https://github.com/informatievlaanderen/road-registry/compare/v3.60.2...v3.60.3) (2023-10-17)


### Bug Fixes

* enable webhost for backoffice hosts healthchecks ([#1243](https://github.com/informatievlaanderen/road-registry/issues/1243)) ([6928914](https://github.com/informatievlaanderen/road-registry/commit/6928914feafdf1274dd3a06cdac47308399f2276))

## [3.60.2](https://github.com/informatievlaanderen/road-registry/compare/v3.60.1...v3.60.2) (2023-10-17)


### Bug Fixes

* environment header in UI + round coordinates when adding to problem + projector health check ([#1242](https://github.com/informatievlaanderen/road-registry/issues/1242)) ([d63347f](https://github.com/informatievlaanderen/road-registry/commit/d63347f7a83dda356ea9397b97f77844249d4c84))

## [3.60.1](https://github.com/informatievlaanderen/road-registry/compare/v3.60.0...v3.60.1) (2023-10-17)


### Bug Fixes

* lambda create change outline geometry deal with no lanes ([#1241](https://github.com/informatievlaanderen/road-registry/issues/1241)) ([e3d98ba](https://github.com/informatievlaanderen/road-registry/commit/e3d98bae5cb5cde893a570d0b91483925291dba1))
* WR-863 temp disable webhost for backoffice hosts ([#1240](https://github.com/informatievlaanderen/road-registry/issues/1240)) ([8db9506](https://github.com/informatievlaanderen/road-registry/commit/8db95065f89aa4f3199a126c9ca4db479d7a766d))

# [3.60.0](https://github.com/informatievlaanderen/road-registry/compare/v3.59.17...v3.60.0) (2023-10-16)


### Bug Fixes

* return 400 when extract upload validation fails ([#1238](https://github.com/informatievlaanderen/road-registry/issues/1238)) ([f847939](https://github.com/informatievlaanderen/road-registry/commit/f84793956e3040da16c94b0186db3133096bfe88))
* WR-865 only close expired extract downloads, not the entire extract ([#1231](https://github.com/informatievlaanderen/road-registry/issues/1231)) ([a9c696b](https://github.com/informatievlaanderen/road-registry/commit/a9c696bd8409eaa748b13c99204dd0309db008ec))


### Features

* WR-863 add healthchecks endpoint to backoffice hosts ([#1230](https://github.com/informatievlaanderen/road-registry/issues/1230)) ([0e4110e](https://github.com/informatievlaanderen/road-registry/commit/0e4110ef8b4e63e320346c26830f744ba515879f))

## [3.59.17](https://github.com/informatievlaanderen/road-registry/compare/v3.59.16...v3.59.17) (2023-10-11)


### Bug Fixes

* build solution, tests & package versions ([#1226](https://github.com/informatievlaanderen/road-registry/issues/1226)) ([c2d81b4](https://github.com/informatievlaanderen/road-registry/commit/c2d81b4be42b82d26590e7d29d37d3dc43ec1dd0))
* reduce conversions from double to decimal ([#1227](https://github.com/informatievlaanderen/road-registry/issues/1227)) ([57d34a1](https://github.com/informatievlaanderen/road-registry/commit/57d34a1fab9cf6e8b6c17434438337ea349c473b))
* WR-621 add missing lanes/surfaces/widths when correcting roadsegment status dutch translations ([#1229](https://github.com/informatievlaanderen/road-registry/issues/1229)) ([6c6c7fb](https://github.com/informatievlaanderen/road-registry/commit/6c6c7fbbdde82fd457df045a81527b4bcf13514a))
* WR-710 add featureflag to toggle GradeSeparatedJunction validation ([#1228](https://github.com/informatievlaanderen/road-registry/issues/1228)) ([8b845b0](https://github.com/informatievlaanderen/road-registry/commit/8b845b0ebe05258fa392803908e5d0c8d594bd08))

## [3.59.16](https://github.com/informatievlaanderen/road-registry/compare/v3.59.15...v3.59.16) (2023-10-10)


### Bug Fixes

* WR-856 only try to fill in TOTPOS when geometry is present ([#1225](https://github.com/informatievlaanderen/road-registry/issues/1225)) ([455071b](https://github.com/informatievlaanderen/road-registry/commit/455071b50856ba61850e71bf4e4d5d1890f939c7))

## [3.59.15](https://github.com/informatievlaanderen/road-registry/compare/v3.59.14...v3.59.15) (2023-10-10)


### Bug Fixes

* always register an IExtractUploadFailedEmailClient implementation ([#1223](https://github.com/informatievlaanderen/road-registry/issues/1223)) ([c72720c](https://github.com/informatievlaanderen/road-registry/commit/c72720ce8e171b7789b71b097db7f4de57356a1c))
* only resolve AmazonSimpleEmailServiceV2Client when it's needed ([#1224](https://github.com/informatievlaanderen/road-registry/issues/1224)) ([24ee5df](https://github.com/informatievlaanderen/road-registry/commit/24ee5dfa641726eb0c59519dcf9366ce079523c3))
* set correct version in lambda deploy pipeline ([#1222](https://github.com/informatievlaanderen/road-registry/issues/1222)) ([62d6729](https://github.com/informatievlaanderen/road-registry/commit/62d6729029c3f90c90e1dc4916a8c45bccffdc13))

## [3.59.14](https://github.com/informatievlaanderen/road-registry/compare/v3.59.13...v3.59.14) (2023-10-09)


### Bug Fixes

* deploy lambda pipeline ([#1221](https://github.com/informatievlaanderen/road-registry/issues/1221)) ([5cde2e7](https://github.com/informatievlaanderen/road-registry/commit/5cde2e73d93d0d91c90f363cb24fbe7d901117ae))

## [3.59.13](https://github.com/informatievlaanderen/road-registry/compare/v3.59.12...v3.59.13) (2023-10-09)


### Bug Fixes

* add step Prepare Lambda to deployment pipeline ([#1219](https://github.com/informatievlaanderen/road-registry/issues/1219)) ([c2f3b5c](https://github.com/informatievlaanderen/road-registry/commit/c2f3b5c1aa670df4776988741298e594c5ec5048))
* disable newly added healthchecks for API ([#1220](https://github.com/informatievlaanderen/road-registry/issues/1220)) ([e05722c](https://github.com/informatievlaanderen/road-registry/commit/e05722c24c7abe834e7fa5c999e5bc55228c334e))

## [3.59.12](https://github.com/informatievlaanderen/road-registry/compare/v3.59.11...v3.59.12) (2023-10-09)


### Bug Fixes

* disable newly added healthchecks on API ([#1215](https://github.com/informatievlaanderen/road-registry/issues/1215)) ([a589788](https://github.com/informatievlaanderen/road-registry/commit/a589788cab2ee516d6854994ed2d1e33eec502e1))
* Healthchecks pass ([#1216](https://github.com/informatievlaanderen/road-registry/issues/1216)) ([391a97e](https://github.com/informatievlaanderen/road-registry/commit/391a97e37f092bb24d131cbfd13626e260afeb42))

## [3.59.11](https://github.com/informatievlaanderen/road-registry/compare/v3.59.10...v3.59.11) (2023-10-09)


### Bug Fixes

* WR-859 casting parameter type from CustomState ([#1214](https://github.com/informatievlaanderen/road-registry/issues/1214)) ([5495be0](https://github.com/informatievlaanderen/road-registry/commit/5495be0f0e59fc35b2114510a3e1c4c95a12ef21))

## [3.59.10](https://github.com/informatievlaanderen/road-registry/compare/v3.59.9...v3.59.10) (2023-09-26)


### Bug Fixes

* Recreate label for roadsegment status ([#1207](https://github.com/informatievlaanderen/road-registry/issues/1207)) ([1935f65](https://github.com/informatievlaanderen/road-registry/commit/1935f65f29eafda394d44c8da1e4082bbe9459d6))
* Remove null webhook URL from the appsettings files ([#1208](https://github.com/informatievlaanderen/road-registry/issues/1208)) ([23e0e23](https://github.com/informatievlaanderen/road-registry/commit/23e0e23c360276c447c56038776b1231e4cb0f56))

## [3.59.9](https://github.com/informatievlaanderen/road-registry/compare/v3.59.8...v3.59.9) (2023-09-21)


### Bug Fixes

* Add shell appsettings to test library ([#1203](https://github.com/informatievlaanderen/road-registry/issues/1203)) ([94ac694](https://github.com/informatievlaanderen/road-registry/commit/94ac6948cb596412edfbe4fa8245900530fe9036))
* deploy pipeline lambda deploy ([#1201](https://github.com/informatievlaanderen/road-registry/issues/1201)) ([98fda15](https://github.com/informatievlaanderen/road-registry/commit/98fda15c95a60eef1fea4d4f0591ed0558eaa235))
* let release pipeline finish when TST deploy is completed ([#1204](https://github.com/informatievlaanderen/road-registry/issues/1204)) ([01dd134](https://github.com/informatievlaanderen/road-registry/commit/01dd134e141b1ff9a355471f341fad6e2fded083))
* Updated lambda email client and appsettings solution wide ([#1202](https://github.com/informatievlaanderen/road-registry/issues/1202)) ([87d5037](https://github.com/informatievlaanderen/road-registry/commit/87d5037467f4b364b1fcf64b0a9d7fc63bd3e54c))
* WR-830 color theme for tst/stg ([#1205](https://github.com/informatievlaanderen/road-registry/issues/1205)) ([ba15b07](https://github.com/informatievlaanderen/road-registry/commit/ba15b077b0d520f9eea0773e9054ed40184979dd))

## [3.59.8](https://github.com/informatievlaanderen/road-registry/compare/v3.59.7...v3.59.8) (2023-09-20)


### Bug Fixes

* bulk update roadsegments for given IDs ([#1200](https://github.com/informatievlaanderen/road-registry/issues/1200)) ([50a36e6](https://github.com/informatievlaanderen/road-registry/commit/50a36e6aaed7e22722b455c0b336aaea2d187caa))
* lambda deploy to newprd ([#1199](https://github.com/informatievlaanderen/road-registry/issues/1199)) ([e7f79a8](https://github.com/informatievlaanderen/road-registry/commit/e7f79a80ace23578962499d459a40f1dc1090add))
* pipeline structure ([#1198](https://github.com/informatievlaanderen/road-registry/issues/1198)) ([7a4c26b](https://github.com/informatievlaanderen/road-registry/commit/7a4c26b28cd53961bdc3e644ebbc75241958a0cf))
* update Be.Vlaanderen.Basisregisters.Aws.DistributedMutex ([#1197](https://github.com/informatievlaanderen/road-registry/issues/1197)) ([eacb284](https://github.com/informatievlaanderen/road-registry/commit/eacb284acfdaf3c1aaa0f191d522359c33556fca))

## [3.59.7](https://github.com/informatievlaanderen/road-registry/compare/v3.59.6...v3.59.7) (2023-09-20)


### Bug Fixes

* add MaxStreamVersion to rebuild snapshot ([#1194](https://github.com/informatievlaanderen/road-registry/issues/1194)) ([b78fd4a](https://github.com/informatievlaanderen/road-registry/commit/b78fd4aca047bdb6120be9579939c8af2e84b295))
* add single servide deploy pipeline ([#1195](https://github.com/informatievlaanderen/road-registry/issues/1195)) ([e78bb40](https://github.com/informatievlaanderen/road-registry/commit/e78bb406e631c47c2fd26f36d55a40093641cfc6))
* enable UI in manual deploy ([#1192](https://github.com/informatievlaanderen/road-registry/issues/1192)) ([42d20ac](https://github.com/informatievlaanderen/road-registry/commit/42d20ac0976ff9179575f96bb0448c1f490069bf))
* remove MessagingHost.Sqs from pipelines ([#1193](https://github.com/informatievlaanderen/road-registry/issues/1193)) ([ae7aa1f](https://github.com/informatievlaanderen/road-registry/commit/ae7aa1f243cbb52878f3e75c126badf1185a222a))
* remove partial deploy ([#1196](https://github.com/informatievlaanderen/road-registry/issues/1196)) ([061cea5](https://github.com/informatievlaanderen/road-registry/commit/061cea57f1dd8bb7ed8bdae2b17ca2eb5013ff12))

## [3.59.6](https://github.com/informatievlaanderen/road-registry/compare/v3.59.5...v3.59.6) (2023-09-19)


### Bug Fixes

* WR-689 remove ApiKeyAttribute ([#1191](https://github.com/informatievlaanderen/road-registry/issues/1191)) ([07f36ed](https://github.com/informatievlaanderen/road-registry/commit/07f36edd56690b32e8cc7f57d34a1db739cbb6e0))

## [3.59.5](https://github.com/informatievlaanderen/road-registry/compare/v3.59.4...v3.59.5) (2023-09-18)


### Bug Fixes

* WR-689 set policies for apikey auth attributes ([#1190](https://github.com/informatievlaanderen/road-registry/issues/1190)) ([6163784](https://github.com/informatievlaanderen/road-registry/commit/616378425ad4e28d1bd724e74443e20c0d1d1144))

## [3.59.4](https://github.com/informatievlaanderen/road-registry/compare/v3.59.3...v3.59.4) (2023-09-18)


### Bug Fixes

* WR-758 performance for missing gradeseparatedjunctions at roadsegment intersections ([#1186](https://github.com/informatievlaanderen/road-registry/issues/1186)) ([2f1ec32](https://github.com/informatievlaanderen/road-registry/commit/2f1ec32ffc97f4fff8f7b264a538a0c3b75cfc3e))

## [3.59.3](https://github.com/informatievlaanderen/road-registry/compare/v3.59.2...v3.59.3) (2023-09-15)


### Bug Fixes

* Add configuration and updated application definitions ([#1185](https://github.com/informatievlaanderen/road-registry/issues/1185)) ([f027796](https://github.com/informatievlaanderen/road-registry/commit/f027796456c5984005d14fb4a75660b39b707a57))
* Send email for RoadNetwork ([#1184](https://github.com/informatievlaanderen/road-registry/issues/1184)) ([b07eabb](https://github.com/informatievlaanderen/road-registry/commit/b07eabb948119400b91b34ceb7aeb02f9a5db4c9))

## [3.59.2](https://github.com/informatievlaanderen/road-registry/compare/v3.59.1...v3.59.2) (2023-09-14)


### Bug Fixes

* Remove file listing ([#1182](https://github.com/informatievlaanderen/road-registry/issues/1182)) ([4c15d6b](https://github.com/informatievlaanderen/road-registry/commit/4c15d6bd6544030d760c6ef846a2b8947a9931d0))
* set Accept header for StreetNameRegistry requests ([#1183](https://github.com/informatievlaanderen/road-registry/issues/1183)) ([1af80b9](https://github.com/informatievlaanderen/road-registry/commit/1af80b946807555427403e30ea1f98e3d99bad91))

## [3.59.1](https://github.com/informatievlaanderen/road-registry/compare/v3.59.0...v3.59.1) (2023-09-14)


### Bug Fixes

* add featuretoggle UseValidateRoadSegmentIntersectionsWithMissingGradeSeparatedJunction ([#1180](https://github.com/informatievlaanderen/road-registry/issues/1180)) ([08f0401](https://github.com/informatievlaanderen/road-registry/commit/08f04019672a527265da80b13fd29c72826d52b2))
* Add NPM token into pipeline ([#1179](https://github.com/informatievlaanderen/road-registry/issues/1179)) ([85df89c](https://github.com/informatievlaanderen/road-registry/commit/85df89c8c74fe7f1072892e26267074998bba047))
* Removed double paket install ([#1176](https://github.com/informatievlaanderen/road-registry/issues/1176)) ([d5dda9e](https://github.com/informatievlaanderen/road-registry/commit/d5dda9e459cbbef7171d48ec8dd4cfe0770d4b41))
* UI pipeline containerize ([#1181](https://github.com/informatievlaanderen/road-registry/issues/1181)) ([7b7d680](https://github.com/informatievlaanderen/road-registry/commit/7b7d6801cbc51fdd369c7207aeb844919043c298))
* UI pipeline requirement for build.fsx ([#1174](https://github.com/informatievlaanderen/road-registry/issues/1174)) ([5a91891](https://github.com/informatievlaanderen/road-registry/commit/5a91891efb8add4c488b346bd7527d8fbc972472))
* Unauthenticated npm repo ([#1178](https://github.com/informatievlaanderen/road-registry/issues/1178)) ([38a3bc2](https://github.com/informatievlaanderen/road-registry/commit/38a3bc2277d11cc333c1fd679da563f7aac4106d))
* Use build.sh publish for UI pipeline ([#1175](https://github.com/informatievlaanderen/road-registry/issues/1175)) ([355e887](https://github.com/informatievlaanderen/road-registry/commit/355e8870638307bf397f6e1d695dfe17dc692c2c))
* Use containerize from fsx file ([#1173](https://github.com/informatievlaanderen/road-registry/issues/1173)) ([8abc3bb](https://github.com/informatievlaanderen/road-registry/commit/8abc3bbac90423df32d98a6519aa255160952459))

# [3.59.0](https://github.com/informatievlaanderen/road-registry/compare/v3.58.9...v3.59.0) (2023-09-14)


### Bug Fixes

* Include UI build ([#1165](https://github.com/informatievlaanderen/road-registry/issues/1165)) ([90da2d3](https://github.com/informatievlaanderen/road-registry/commit/90da2d3bb8bba3a270d9a09f81b2aa6dfe9e9590))
* UI pipeline ([#1166](https://github.com/informatievlaanderen/road-registry/issues/1166)) ([618d78b](https://github.com/informatievlaanderen/road-registry/commit/618d78bf5299547d8b8fb51b34d3294d148f1935))
* UI pipeline ([#1167](https://github.com/informatievlaanderen/road-registry/issues/1167)) ([ec1a449](https://github.com/informatievlaanderen/road-registry/commit/ec1a449cd9033e54526b6f28da0f0f033fe71079))
* UI pipeline ([#1168](https://github.com/informatievlaanderen/road-registry/issues/1168)) ([d166de6](https://github.com/informatievlaanderen/road-registry/commit/d166de63377adbac65494ad9703a4f93a6fdc572))
* UI pipeline ([#1171](https://github.com/informatievlaanderen/road-registry/issues/1171)) ([1fba20f](https://github.com/informatievlaanderen/road-registry/commit/1fba20f2dcf6de3b0730e382c818de62d976de36))
* UI pipeline ([#1172](https://github.com/informatievlaanderen/road-registry/issues/1172)) ([ddd7e5b](https://github.com/informatievlaanderen/road-registry/commit/ddd7e5b9b8b1fd66ae29d9f8e1ea57234b0b9698))
* WR-783 add shape reader/writer using NetTopologySuite ([#1164](https://github.com/informatievlaanderen/road-registry/issues/1164)) ([a324e3f](https://github.com/informatievlaanderen/road-registry/commit/a324e3f16b534eaceaf42cc6b53343b2ced00dd6))


### Features

* WR-833 use StreetNameClient instead of kafka cache for write operations ([#1170](https://github.com/informatievlaanderen/road-registry/issues/1170)) ([caec8f1](https://github.com/informatievlaanderen/road-registry/commit/caec8f1344265291ccf8b1a5fc219f8e5532b7dc))

## [3.58.9](https://github.com/informatievlaanderen/road-registry/compare/v3.58.8...v3.58.9) (2023-09-13)


### Bug Fixes

* UI pipeline ([#1163](https://github.com/informatievlaanderen/road-registry/issues/1163)) ([448bee9](https://github.com/informatievlaanderen/road-registry/commit/448bee93dea6517d557a74ef86905751c9428eb3))

## [3.58.8](https://github.com/informatievlaanderen/road-registry/compare/v3.58.7...v3.58.8) (2023-09-13)


### Bug Fixes

* FC GradeSeparatedJunction validation messages ([#1162](https://github.com/informatievlaanderen/road-registry/issues/1162)) ([647fc2b](https://github.com/informatievlaanderen/road-registry/commit/647fc2b0f61e58ed5e5a8518c1fa1019e47e77de))

## [3.58.7](https://github.com/informatievlaanderen/road-registry/compare/v3.58.6...v3.58.7) (2023-09-13)


### Bug Fixes

* Add nodejs pipeline code ([#1156](https://github.com/informatievlaanderen/road-registry/issues/1156)) ([e5ad849](https://github.com/informatievlaanderen/road-registry/commit/e5ad849718d3a035cbb8f2701d97cf8eef9e296d))
* translation message + add featuretoggle override in API through request ([#1158](https://github.com/informatievlaanderen/road-registry/issues/1158)) ([cb27a27](https://github.com/informatievlaanderen/road-registry/commit/cb27a27229380922d504b1e6981e500b308c7e9e))
* UI pipeline ([#1159](https://github.com/informatievlaanderen/road-registry/issues/1159)) ([f987492](https://github.com/informatievlaanderen/road-registry/commit/f987492308216e0d7ae097883b14d3741f4629d6))
* UI pipeline ([#1161](https://github.com/informatievlaanderen/road-registry/issues/1161)) ([1337f71](https://github.com/informatievlaanderen/road-registry/commit/1337f718b58f1accbbcb3c0da1c8b395837b33b5))
* Update UI pipeline ([#1160](https://github.com/informatievlaanderen/road-registry/issues/1160)) ([acd0631](https://github.com/informatievlaanderen/road-registry/commit/acd06312868b7cb7d602f8e95ef65618199ac3d1))

## [3.58.6](https://github.com/informatievlaanderen/road-registry/compare/v3.58.5...v3.58.6) (2023-09-12)


### Bug Fixes

* handle ZipArchiveValidationException with extracts upload endpoint ([#1157](https://github.com/informatievlaanderen/road-registry/issues/1157)) ([e50db26](https://github.com/informatievlaanderen/road-registry/commit/e50db26626756563956cdaa749396eb6c0f6dc17))

## [3.58.5](https://github.com/informatievlaanderen/road-registry/compare/v3.58.4...v3.58.5) (2023-09-11)


### Bug Fixes

* prd/newprd lambda environment ([#1154](https://github.com/informatievlaanderen/road-registry/issues/1154)) ([af793a1](https://github.com/informatievlaanderen/road-registry/commit/af793a17eb62e656bf78e3fba834a7e582a3ba4f))

## [3.58.4](https://github.com/informatievlaanderen/road-registry/compare/v3.58.3...v3.58.4) (2023-09-08)


### Bug Fixes

* Send mail message after Rejected been applied ([#1152](https://github.com/informatievlaanderen/road-registry/issues/1152)) ([9213409](https://github.com/informatievlaanderen/road-registry/commit/92134091fc4574d0b4ad575820456287fe1d566d))
* treat empty files during upload always as warning ([#1153](https://github.com/informatievlaanderen/road-registry/issues/1153)) ([532e85f](https://github.com/informatievlaanderen/road-registry/commit/532e85f69e1a41215390d2193f56d5dc4377580f))

## [3.58.3](https://github.com/informatievlaanderen/road-registry/compare/v3.58.2...v3.58.3) (2023-09-08)


### Bug Fixes

* WR-752 add featuretoggle UseOvoCodeInChangeRoadNetwork ([#1151](https://github.com/informatievlaanderen/road-registry/issues/1151)) ([f860e8d](https://github.com/informatievlaanderen/road-registry/commit/f860e8d26b8759eccf2f4c2ea9c612516cb66855))

## [3.58.2](https://github.com/informatievlaanderen/road-registry/compare/v3.58.1...v3.58.2) (2023-09-07)


### Bug Fixes

* WR-774 exclude intersections of segments and their endnodes ([#1150](https://github.com/informatievlaanderen/road-registry/issues/1150)) ([8a54c23](https://github.com/informatievlaanderen/road-registry/commit/8a54c236a4990a9e396afa7eef088745f3d161ac))

## [3.58.1](https://github.com/informatievlaanderen/road-registry/compare/v3.58.0...v3.58.1) (2023-09-06)


### Bug Fixes

* add newprd to release pipeline ([#1149](https://github.com/informatievlaanderen/road-registry/issues/1149)) ([9ef50d6](https://github.com/informatievlaanderen/road-registry/commit/9ef50d6f2cf6edee7cccfa38b665ff4459b63071))

# [3.58.0](https://github.com/informatievlaanderen/road-registry/compare/v3.57.3...v3.58.0) (2023-09-06)


### Bug Fixes

* WR-774 new FC, delete roadnodes which are added with the same ID ([#1147](https://github.com/informatievlaanderen/road-registry/issues/1147)) ([4af9fd9](https://github.com/informatievlaanderen/road-registry/commit/4af9fd965164fce46b19ed05fa3bd0c28ee1753c))


### Features

* WR-758 WR-710 add GradeSeparatedJunction validations ([#1148](https://github.com/informatievlaanderen/road-registry/issues/1148)) ([134143a](https://github.com/informatievlaanderen/road-registry/commit/134143a39d3aba1b29745e029d0073f446f708c3))
* WR-813 Add identity information on endpoint requests, SQS requests and commands ([#1143](https://github.com/informatievlaanderen/road-registry/issues/1143)) ([efc9d3a](https://github.com/informatievlaanderen/road-registry/commit/efc9d3ac04509bf3ed72e7988eb627920497b400))

## [3.57.3](https://github.com/informatievlaanderen/road-registry/compare/v3.57.2...v3.57.3) (2023-09-01)


### Bug Fixes

* add ProductContext to OrganizationNames correction ([#1145](https://github.com/informatievlaanderen/road-registry/issues/1145)) ([9663185](https://github.com/informatievlaanderen/road-registry/commit/96631856f7eacdf293a6831a40c1d2ee07820616))

## [3.57.2](https://github.com/informatievlaanderen/road-registry/compare/v3.57.1...v3.57.2) (2023-09-01)


### Bug Fixes

* add logging to streetnameconsumer ([#1144](https://github.com/informatievlaanderen/road-registry/issues/1144)) ([82e14ed](https://github.com/informatievlaanderen/road-registry/commit/82e14ed20c711e38e5ca6307e6265bc1a4b93c16))

## [3.57.1](https://github.com/informatievlaanderen/road-registry/compare/v3.57.0...v3.57.1) (2023-08-31)


### Bug Fixes

* add roadnode id to translated error message ([#1141](https://github.com/informatievlaanderen/road-registry/issues/1141)) ([d05373b](https://github.com/informatievlaanderen/road-registry/commit/d05373b49ecc1363d6e4a17696c8f919365f270d))
* WR-792 OrganizationConsumer stop trying when ConfigurationErrorsExcepti… ([#1142](https://github.com/informatievlaanderen/road-registry/issues/1142)) ([693fff1](https://github.com/informatievlaanderen/road-registry/commit/693fff19bbb9e4cad124c39a731a9fbd5e82babe))

# [3.57.0](https://github.com/informatievlaanderen/road-registry/compare/v3.56.13...v3.57.0) (2023-08-30)


### Bug Fixes

* manual pipeline lambda credentials ([#1138](https://github.com/informatievlaanderen/road-registry/issues/1138)) ([623d988](https://github.com/informatievlaanderen/road-registry/commit/623d988ac8b8b6caa7976a1654ad96b6d5a6a2fd))
* WR-792 keep long names of organizations during sync + error handling of ZipArchiveValidationException with new FC ([#1140](https://github.com/informatievlaanderen/road-registry/issues/1140)) ([c236735](https://github.com/informatievlaanderen/road-registry/commit/c2367351cb1c557f0b9422d06f38a31f9ec5aa3c))


### Features

* add SyncHost with OrganizationConsumer ([#1139](https://github.com/informatievlaanderen/road-registry/issues/1139)) ([a01e691](https://github.com/informatievlaanderen/road-registry/commit/a01e691c30d10fab706799d0002d40a48bf5a19c))
* WR-752 Capture administrative actions origin provenance ([#1113](https://github.com/informatievlaanderen/road-registry/issues/1113)) ([dae03e6](https://github.com/informatievlaanderen/road-registry/commit/dae03e639cb56f9037d8e9bc381addc5bf3cb940))

## [3.56.13](https://github.com/informatievlaanderen/road-registry/compare/v3.56.12...v3.56.13) (2023-08-24)


### Bug Fixes

* ignore validation errors for integration data ([#1135](https://github.com/informatievlaanderen/road-registry/issues/1135)) ([98734d8](https://github.com/informatievlaanderen/road-registry/commit/98734d8945411e5825a8d67961f7b84198ec35fc))

## [3.56.12](https://github.com/informatievlaanderen/road-registry/compare/v3.56.11...v3.56.12) (2023-08-23)


### Bug Fixes

* release v2 production steps order ([#1134](https://github.com/informatievlaanderen/road-registry/issues/1134)) ([71c482f](https://github.com/informatievlaanderen/road-registry/commit/71c482f52b0393662e3adbfb431cc879f9bee1f9))

## [3.56.11](https://github.com/informatievlaanderen/road-registry/compare/v3.56.10...v3.56.11) (2023-08-23)


### Bug Fixes

* pipeline lambda TST functionnames ([#1133](https://github.com/informatievlaanderen/road-registry/issues/1133)) ([1272abe](https://github.com/informatievlaanderen/road-registry/commit/1272abe4626521c076e44b64a1cf586ab2682f89))

## [3.56.10](https://github.com/informatievlaanderen/road-registry/compare/v3.56.9...v3.56.10) (2023-08-23)


### Bug Fixes

* add missing docker network creation to run script ([#1131](https://github.com/informatievlaanderen/road-registry/issues/1131)) ([2424a93](https://github.com/informatievlaanderen/road-registry/commit/2424a93f1ac6b55b1edc5191fcd154300384e4f9))
* release v2 stg approval ([#1132](https://github.com/informatievlaanderen/road-registry/issues/1132)) ([c672368](https://github.com/informatievlaanderen/road-registry/commit/c6723689ff2fa4329ab7ca5d190e5dd6e358bef8))

## [3.56.9](https://github.com/informatievlaanderen/road-registry/compare/v3.56.8...v3.56.9) (2023-08-23)


### Bug Fixes

* release pipeline lambda function names ([#1130](https://github.com/informatievlaanderen/road-registry/issues/1130)) ([fb29724](https://github.com/informatievlaanderen/road-registry/commit/fb29724a118fc3216bc5699df9f62d2adbf0adf9))

## [3.56.8](https://github.com/informatievlaanderen/road-registry/compare/v3.56.7...v3.56.8) (2023-08-18)


### Bug Fixes

* upload lambdas in pipeline ([dc1aef7](https://github.com/informatievlaanderen/road-registry/commit/dc1aef7bdac7f9aff0bed37b323ea62d31576285))

## [3.56.7](https://github.com/informatievlaanderen/road-registry/compare/v3.56.6...v3.56.7) (2023-08-18)


### Bug Fixes

* push to staging and deploy ([8287985](https://github.com/informatievlaanderen/road-registry/commit/828798580826e5a76d5c5a535bc6da32fb16e73e))

## [3.56.6](https://github.com/informatievlaanderen/road-registry/compare/v3.56.5...v3.56.6) (2023-08-18)


### Bug Fixes

* deploy steps pipeline ([9f38d79](https://github.com/informatievlaanderen/road-registry/commit/9f38d792f51195d8be495bfbd03fad50afff1f55))

## [3.56.5](https://github.com/informatievlaanderen/road-registry/compare/v3.56.4...v3.56.5) (2023-08-18)


### Bug Fixes

* Pre-init release V2 pipeline ([#1127](https://github.com/informatievlaanderen/road-registry/issues/1127)) ([5b593f4](https://github.com/informatievlaanderen/road-registry/commit/5b593f44927accb222efadba6619c0679ac4764b))

## [3.56.4](https://github.com/informatievlaanderen/road-registry/compare/v3.56.3...v3.56.4) (2023-08-11)


### Bug Fixes

* build script add open system.io ([#1119](https://github.com/informatievlaanderen/road-registry/issues/1119)) ([fc964dd](https://github.com/informatievlaanderen/road-registry/commit/fc964dd89d2674bb73ac872b293cd265f371ee91))
* build script define array syntax ([#1120](https://github.com/informatievlaanderen/road-registry/issues/1120)) ([e06c778](https://github.com/informatievlaanderen/road-registry/commit/e06c7789dae520bcab30d994117516e5cce99fe6))
* ensure init.sh in docker image is the correct one ([#1117](https://github.com/informatievlaanderen/road-registry/issues/1117)) ([788dabe](https://github.com/informatievlaanderen/road-registry/commit/788dabec7c6cd7247767ae3e27d5858b9a00e67b))
* only copy init.sh if it exists ([#1118](https://github.com/informatievlaanderen/road-registry/issues/1118)) ([d67264f](https://github.com/informatievlaanderen/road-registry/commit/d67264f73eb1ebb08115d225e887beb872f756f6))
* WR-622+785 roadsegment organization name cannot be empty + update java version for build ([#1116](https://github.com/informatievlaanderen/road-registry/issues/1116)) ([8d114df](https://github.com/informatievlaanderen/road-registry/commit/8d114df07735a0abbdaef2b85c6ecce75bad1577))

## [3.56.3](https://github.com/informatievlaanderen/road-registry/compare/v3.56.2...v3.56.3) (2023-08-11)


### Bug Fixes

* pipeline disable step 'Push Lambda functions to S3 New Production' ([#1115](https://github.com/informatievlaanderen/road-registry/issues/1115)) ([24a1015](https://github.com/informatievlaanderen/road-registry/commit/24a10157e2c3f4ca413c831a16bdb2d88edd8709))

## [3.56.2](https://github.com/informatievlaanderen/road-registry/compare/v3.56.1...v3.56.2) (2023-08-10)


### Bug Fixes

* newprd lambda s3 bucket names ([#1114](https://github.com/informatievlaanderen/road-registry/issues/1114)) ([e4e35fa](https://github.com/informatievlaanderen/road-registry/commit/e4e35fa1918d073900a758a882887c64e758781b))

## [3.56.1](https://github.com/informatievlaanderen/road-registry/compare/v3.56.0...v3.56.1) (2023-08-10)


### Bug Fixes

* add manual deploy pipeline ([#1106](https://github.com/informatievlaanderen/road-registry/issues/1106)) ([784cdd8](https://github.com/informatievlaanderen/road-registry/commit/784cdd840d5e3d8e769c8a0967ce81b868d29fe9))
* add newprd to release ([#1110](https://github.com/informatievlaanderen/road-registry/issues/1110)) ([5fa3f5b](https://github.com/informatievlaanderen/road-registry/commit/5fa3f5b308d4b1e16cd13e4c954c681635114158))
* manual deploy pipeline rename `acc` to `newprd` ([#1109](https://github.com/informatievlaanderen/road-registry/issues/1109)) ([a387581](https://github.com/informatievlaanderen/road-registry/commit/a387581964533cf406587a7934ad88c8b108adf0))
* manual deployment pipeline ([#1107](https://github.com/informatievlaanderen/road-registry/issues/1107)) ([93774b6](https://github.com/informatievlaanderen/road-registry/commit/93774b6957592c5b003b413fa790af0fdd2785e4))
* manual pipeline lambda deploy ([#1108](https://github.com/informatievlaanderen/road-registry/issues/1108)) ([b786de9](https://github.com/informatievlaanderen/road-registry/commit/b786de9343a15442850ceb82eaf822ec5fae93b4))
* remove services matrix for build ([#1112](https://github.com/informatievlaanderen/road-registry/issues/1112)) ([2abd84d](https://github.com/informatievlaanderen/road-registry/commit/2abd84d7ef8fb6469a4fca7699ecac9e193245f5))
* revert use matrix for nuget packages ([#1111](https://github.com/informatievlaanderen/road-registry/issues/1111)) ([10e5962](https://github.com/informatievlaanderen/road-registry/commit/10e5962a0b9505756310c219008a282d316dcb51))

# [3.56.0](https://github.com/informatievlaanderen/road-registry/compare/v3.55.0...v3.56.0) (2023-08-08)


### Features

* WR-753 add endpoint to retrieve organizations ([#1105](https://github.com/informatievlaanderen/road-registry/issues/1105)) ([32fac39](https://github.com/informatievlaanderen/road-registry/commit/32fac39f2bc62693e8f08ec78da7ab89c6c2006c))
* WR-762 add OVO-code to organization ([#1104](https://github.com/informatievlaanderen/road-registry/issues/1104)) ([c242e73](https://github.com/informatievlaanderen/road-registry/commit/c242e736d47c600f00fceffec69d5b64c67203c8))

# [3.55.0](https://github.com/informatievlaanderen/road-registry/compare/v3.54.14...v3.55.0) (2023-08-04)


### Features

* WR-632 consume streetname snapshot kafka topic ([#1098](https://github.com/informatievlaanderen/road-registry/issues/1098)) ([05fa885](https://github.com/informatievlaanderen/road-registry/commit/05fa8858757ec97589aa9082a5010eec93a24b13))
* WR-754 Add meaningful retry after estimation time ([#1097](https://github.com/informatievlaanderen/road-registry/issues/1097)) ([9ce8197](https://github.com/informatievlaanderen/road-registry/commit/9ce819788af850ffa50b2b77d01f20b198f4bcb2))

## [3.54.14](https://github.com/informatievlaanderen/road-registry/compare/v3.54.13...v3.54.14) (2023-08-03)


### Bug Fixes

* release pipeline missing --sse parameter ([#1103](https://github.com/informatievlaanderen/road-registry/issues/1103)) ([7771dfd](https://github.com/informatievlaanderen/road-registry/commit/7771dfd7c8bc3398b64ae2634805ff1d29dce628))

## [3.54.13](https://github.com/informatievlaanderen/road-registry/compare/v3.54.12...v3.54.13) (2023-08-03)


### Bug Fixes

* test s3 lambda bucket names ([#1102](https://github.com/informatievlaanderen/road-registry/issues/1102)) ([27d5ec8](https://github.com/informatievlaanderen/road-registry/commit/27d5ec83b48239469a6f32b1ea880eeba16b783a))

## [3.54.12](https://github.com/informatievlaanderen/road-registry/compare/v3.54.11...v3.54.12) (2023-08-03)


### Bug Fixes

* WR-756 endpoint auth scopes ([#1099](https://github.com/informatievlaanderen/road-registry/issues/1099)) ([b14a2d3](https://github.com/informatievlaanderen/road-registry/commit/b14a2d399d986781b4dabc94e7627470ea1eae9b))

## [3.54.11](https://github.com/informatievlaanderen/road-registry/compare/v3.54.10...v3.54.11) (2023-08-02)


### Bug Fixes

* WR-689 redirect url on logout ([#1096](https://github.com/informatievlaanderen/road-registry/issues/1096)) ([7e8466d](https://github.com/informatievlaanderen/road-registry/commit/7e8466d0c4cd7a612964abf3eb0d6fdb3c160848))

## [3.54.10](https://github.com/informatievlaanderen/road-registry/compare/v3.54.9...v3.54.10) (2023-08-01)


### Bug Fixes

* logout with ACM/IDM ([#1095](https://github.com/informatievlaanderen/road-registry/issues/1095)) ([f498cc0](https://github.com/informatievlaanderen/road-registry/commit/f498cc054114dece1953d16e440e3e9798beef93))

## [3.54.9](https://github.com/informatievlaanderen/road-registry/compare/v3.54.8...v3.54.9) (2023-08-01)


### Bug Fixes

* WR-689 remove test handler ([#1094](https://github.com/informatievlaanderen/road-registry/issues/1094)) ([c46a9d6](https://github.com/informatievlaanderen/road-registry/commit/c46a9d6fe33d00675852ee9a2d5c28607e8a3e30))

## [3.54.8](https://github.com/informatievlaanderen/road-registry/compare/v3.54.7...v3.54.8) (2023-07-31)


### Bug Fixes

* WR-689 add test handler for JwtBearer ([#1093](https://github.com/informatievlaanderen/road-registry/issues/1093)) ([e8105fb](https://github.com/informatievlaanderen/road-registry/commit/e8105fb698312f72893afb8c455220d5fd483707))

## [3.54.7](https://github.com/informatievlaanderen/road-registry/compare/v3.54.6...v3.54.7) (2023-07-28)


### Bug Fixes

* remove DOCKER_TMPDIR ([#1092](https://github.com/informatievlaanderen/road-registry/issues/1092)) ([9362eb8](https://github.com/informatievlaanderen/road-registry/commit/9362eb89c185d63873ae357954930b9e62dbef18))

## [3.54.6](https://github.com/informatievlaanderen/road-registry/compare/v3.54.5...v3.54.6) (2023-07-28)


### Bug Fixes

* WR-689 use separate auth scheme for JwtBearer ([#1091](https://github.com/informatievlaanderen/road-registry/issues/1091)) ([a092a8c](https://github.com/informatievlaanderen/road-registry/commit/a092a8c2a0aed9648cc7623e8562cec91c6d3a24))

## [3.54.5](https://github.com/informatievlaanderen/road-registry/compare/v3.54.4...v3.54.5) (2023-07-28)


### Bug Fixes

* pipeline delete unnecessary folders ([#1089](https://github.com/informatievlaanderen/road-registry/issues/1089)) ([ac0a32b](https://github.com/informatievlaanderen/road-registry/commit/ac0a32b811de31c0d0e215959fd082d84d81702b))
* pipeline typo ([#1090](https://github.com/informatievlaanderen/road-registry/issues/1090)) ([6c333af](https://github.com/informatievlaanderen/road-registry/commit/6c333af4be0fdaee3484b7b29eb509f8f7570a43))

## [3.54.4](https://github.com/informatievlaanderen/road-registry/compare/v3.54.3...v3.54.4) (2023-07-27)


### Bug Fixes

* WR-689 add auth for jwttoken ([#1088](https://github.com/informatievlaanderen/road-registry/issues/1088)) ([e11fdab](https://github.com/informatievlaanderen/road-registry/commit/e11fdabaa9c851e5899cceae0a9d0ab4addea7d0))
* WR-774 FC validate roadnode ids; correctly use new id when existing is replaced ([#1087](https://github.com/informatievlaanderen/road-registry/issues/1087)) ([c9c2d61](https://github.com/informatievlaanderen/road-registry/commit/c9c2d617c5bf4cdc94168eda743b7f3cec506d12))

## [3.54.3](https://github.com/informatievlaanderen/road-registry/compare/v3.54.2...v3.54.3) (2023-07-26)


### Bug Fixes

* add df -h logging to pipeline ([#1086](https://github.com/informatievlaanderen/road-registry/issues/1086)) ([500867c](https://github.com/informatievlaanderen/road-registry/commit/500867ca0a3e15b5071dedcb919433c1973082ad))

## [3.54.2](https://github.com/informatievlaanderen/road-registry/compare/v3.54.1...v3.54.2) (2023-07-26)


### Bug Fixes

* version bump ([#1085](https://github.com/informatievlaanderen/road-registry/issues/1085)) ([a988dfc](https://github.com/informatievlaanderen/road-registry/commit/a988dfcfbf5d64fd5f099da960faa123158a37e1))

## [3.54.1](https://github.com/informatievlaanderen/road-registry/compare/v3.54.0...v3.54.1) (2023-07-26)


### Bug Fixes

* WR-703 change dynamic attributes examples class ([#1084](https://github.com/informatievlaanderen/road-registry/issues/1084)) ([87589a3](https://github.com/informatievlaanderen/road-registry/commit/87589a35bdbd466d2c3118c4d980f465a6d6e9b7))

# [3.54.0](https://github.com/informatievlaanderen/road-registry/compare/v3.53.4...v3.54.0) (2023-07-25)


### Bug Fixes

* Singular exception message for email client ([#1081](https://github.com/informatievlaanderen/road-registry/issues/1081)) ([f8d580c](https://github.com/informatievlaanderen/road-registry/commit/f8d580c219fd38dafe91b94496fd42d97edfb5e2))
* WR-775 hard delete previously removed surfaces from another roadsegment when adding to a new roadsegment ([#1082](https://github.com/informatievlaanderen/road-registry/issues/1082)) ([90a8151](https://github.com/informatievlaanderen/road-registry/commit/90a8151bbaea1fdd7f688b04c40a15dba52715cf))


### Features

* WR-703 change url of latest change roadsegments endpoint ([#1083](https://github.com/informatievlaanderen/road-registry/issues/1083)) ([184edca](https://github.com/informatievlaanderen/road-registry/commit/184edcabe0ff76a8399597a9cb631a85c87f4d6e))

## [3.53.4](https://github.com/informatievlaanderen/road-registry/compare/v3.53.3...v3.53.4) (2023-07-25)


### Bug Fixes

* Don't crash when emailer fails ([#1080](https://github.com/informatievlaanderen/road-registry/issues/1080)) ([e8627f9](https://github.com/informatievlaanderen/road-registry/commit/e8627f9b6f7b329ac25afe611037a6600ec58e70))
* WR-770 remove special rules for roadsegment attributes outlined ([#1079](https://github.com/informatievlaanderen/road-registry/issues/1079)) ([c2c3cd3](https://github.com/informatievlaanderen/road-registry/commit/c2c3cd31d3b3df32c476db665e4eb138210b8a36))

## [3.53.3](https://github.com/informatievlaanderen/road-registry/compare/v3.53.2...v3.53.3) (2023-07-24)


### Bug Fixes

* WR-775 re-adding previously removed entities in kafka ([#1073](https://github.com/informatievlaanderen/road-registry/issues/1073)) ([3244ea9](https://github.com/informatievlaanderen/road-registry/commit/3244ea9cf1e8db42c7d8f54cb29c5f261ad370db))

## [3.53.2](https://github.com/informatievlaanderen/road-registry/compare/v3.53.1...v3.53.2) (2023-07-19)


### Bug Fixes

* WR-689 read dv_wegenregister claim to determine scopes ([#1072](https://github.com/informatievlaanderen/road-registry/issues/1072)) ([7a4a356](https://github.com/informatievlaanderen/road-registry/commit/7a4a356e77632ecc5ac3843c445e4b86920d19d5))

## [3.53.1](https://github.com/informatievlaanderen/road-registry/compare/v3.53.0...v3.53.1) (2023-07-19)


### Bug Fixes

* WR-689 use any DV scope to check if authenticated ([#1071](https://github.com/informatievlaanderen/road-registry/issues/1071)) ([83bb2a4](https://github.com/informatievlaanderen/road-registry/commit/83bb2a41eff1112fe0755459acba987cea2567cb))

# [3.53.0](https://github.com/informatievlaanderen/road-registry/compare/v3.52.11...v3.53.0) (2023-07-18)


### Bug Fixes

* Add configuration instead of fixed value email sender ([#1066](https://github.com/informatievlaanderen/road-registry/issues/1066)) ([3e00f40](https://github.com/informatievlaanderen/road-registry/commit/3e00f4014a16eb940211e07a6d23eeaf8133f18b))
* add integration data to ExtractsZipArchiveBuilder to ensure test always succeeds ([#1067](https://github.com/informatievlaanderen/road-registry/issues/1067)) ([a9c5ae2](https://github.com/informatievlaanderen/road-registry/commit/a9c5ae264c44b3a209e5f94c3304bffc1dcaaaa2))
* WR-689 add logging ([#1068](https://github.com/informatievlaanderen/road-registry/issues/1068)) ([8bce479](https://github.com/informatievlaanderen/road-registry/commit/8bce479cae7b7636b8aff8b4ab80ff2cc4830d30))


### Features

* WR-759 add validations + use FC feature reader for before-FC validation ([#1065](https://github.com/informatievlaanderen/road-registry/issues/1065)) ([8482382](https://github.com/informatievlaanderen/road-registry/commit/848238225b34a42a9f875988164a202938cabbbb))

## [3.52.11](https://github.com/informatievlaanderen/road-registry/compare/v3.52.10...v3.52.11) (2023-07-18)


### Bug Fixes

* Attempted release pipeline ([#1064](https://github.com/informatievlaanderen/road-registry/issues/1064)) ([a1c9e34](https://github.com/informatievlaanderen/road-registry/commit/a1c9e3448ae3bb098a19d4e5593136cdbbc09a8d))

## [3.52.10](https://github.com/informatievlaanderen/road-registry/compare/v3.52.9...v3.52.10) (2023-07-17)


### Bug Fixes

* Allow release v1 to run ([#1062](https://github.com/informatievlaanderen/road-registry/issues/1062)) ([89ac701](https://github.com/informatievlaanderen/road-registry/commit/89ac701934ec6739afe32e0b5088bea9901dba4a))

## [3.52.9](https://github.com/informatievlaanderen/road-registry/compare/v3.52.8...v3.52.9) (2023-07-17)


### Bug Fixes

* commandhost correctly process change of roadsegment from measured to outlined ([#1061](https://github.com/informatievlaanderen/road-registry/issues/1061)) ([7211d3a](https://github.com/informatievlaanderen/road-registry/commit/7211d3ae33e5cd7977414ee021dc6edd2a6439e4))

## [3.52.8](https://github.com/informatievlaanderen/road-registry/compare/v3.52.7...v3.52.8) (2023-07-17)


### Bug Fixes

* Renamed images to conform with build and deploy ([#1058](https://github.com/informatievlaanderen/road-registry/issues/1058)) ([2326ea6](https://github.com/informatievlaanderen/road-registry/commit/2326ea6a495cf47144a915842b0247939754be08))

## [3.52.7](https://github.com/informatievlaanderen/road-registry/compare/v3.52.6...v3.52.7) (2023-07-10)


### Bug Fixes

* Incorrect build URL ([#1055](https://github.com/informatievlaanderen/road-registry/issues/1055)) ([1cc3f35](https://github.com/informatievlaanderen/road-registry/commit/1cc3f35d6acfd985ba944f7105c198789e305359))

## [3.52.6](https://github.com/informatievlaanderen/road-registry/compare/v3.52.5...v3.52.6) (2023-07-10)


### Bug Fixes

* Incorrrect slack channel and status URL ([#1054](https://github.com/informatievlaanderen/road-registry/issues/1054)) ([7c833b1](https://github.com/informatievlaanderen/road-registry/commit/7c833b12a34b504f8339d3b2791edf56d1c59b69))

## [3.52.5](https://github.com/informatievlaanderen/road-registry/compare/v3.52.4...v3.52.5) (2023-07-07)


### Bug Fixes

* Add set-output otherwise this will be blank ([#1053](https://github.com/informatievlaanderen/road-registry/issues/1053)) ([12e5ed2](https://github.com/informatievlaanderen/road-registry/commit/12e5ed20802f7babbef7bff1c0d36cc928cd37fa))

## [3.52.4](https://github.com/informatievlaanderen/road-registry/compare/v3.52.3...v3.52.4) (2023-07-07)


### Bug Fixes

* Added secret usage and NPM token environment variable ([#1047](https://github.com/informatievlaanderen/road-registry/issues/1047)) ([95de726](https://github.com/informatievlaanderen/road-registry/commit/95de726103bf4c1ab4301f0fcc5088db9cfdae3d))
* Paket templates should be inside output folders ([#1049](https://github.com/informatievlaanderen/road-registry/issues/1049)) ([8e49c08](https://github.com/informatievlaanderen/road-registry/commit/8e49c0898647a1cb1acc9d06ad1df77db6db7933))
* Remove composed container environment variable ([#1048](https://github.com/informatievlaanderen/road-registry/issues/1048)) ([fa6182b](https://github.com/informatievlaanderen/road-registry/commit/fa6182bfc36b3d05aff4401f8ebed0e8e9318992))
* Removed xml notation and fixed paket.template ([#1051](https://github.com/informatievlaanderen/road-registry/issues/1051)) ([5fe25e9](https://github.com/informatievlaanderen/road-registry/commit/5fe25e989e429bf1fb613e5d6cfd742be24a0845))
* Required specific dotnet version for semantic release ([#1052](https://github.com/informatievlaanderen/road-registry/issues/1052)) ([282cd1e](https://github.com/informatievlaanderen/road-registry/commit/282cd1e42ca4281ec9ee85b096f802bccb6ce814))
* when reading extract contour it won't matter if it's polygon/multipolygon ([#1050](https://github.com/informatievlaanderen/road-registry/issues/1050)) ([173513b](https://github.com/informatievlaanderen/road-registry/commit/173513b060843ce63257d23d2dc76f26ed5a6c71))

## [3.52.3](https://github.com/informatievlaanderen/road-registry/compare/v3.52.2...v3.52.3) (2023-07-06)


### Bug Fixes

* Add support for classic and new pipeline ([#1046](https://github.com/informatievlaanderen/road-registry/issues/1046)) ([0688bfc](https://github.com/informatievlaanderen/road-registry/commit/0688bfc16dfdd3fc16d4deeba60fc28b86192018))

## [3.52.2](https://github.com/informatievlaanderen/road-registry/compare/v3.52.1...v3.52.2) (2023-07-06)


### Bug Fixes

* Add environment variables and fix project name ([#1039](https://github.com/informatievlaanderen/road-registry/issues/1039)) ([5d8bab5](https://github.com/informatievlaanderen/road-registry/commit/5d8bab5b6f9218d35fd004bf4fb281186479f154))
* Add multiple release pipeline with dry-run ([#1036](https://github.com/informatievlaanderen/road-registry/issues/1036)) ([6363138](https://github.com/informatievlaanderen/road-registry/commit/63631380a454658bcd88b586dccd245caa226341))
* Added mandatory test-project arguments ([#1041](https://github.com/informatievlaanderen/road-registry/issues/1041)) ([b34949c](https://github.com/informatievlaanderen/road-registry/commit/b34949c91cdac7c3133169923d0a9a9e7d318c07))
* Added NPM token and placed CI marker ([#1037](https://github.com/informatievlaanderen/road-registry/issues/1037)) ([631ab25](https://github.com/informatievlaanderen/road-registry/commit/631ab25e545e2b40f4e0e0bf66d7a05f6551430c))
* lower CatchUpBatchSize to 500 ([#1044](https://github.com/informatievlaanderen/road-registry/issues/1044)) ([56711f1](https://github.com/informatievlaanderen/road-registry/commit/56711f1a758b57ed4ae0e82cd6ca783e87dea66b))
* Reformed package.json for semantic release dry-runs ([#1038](https://github.com/informatievlaanderen/road-registry/issues/1038)) ([8233da5](https://github.com/informatievlaanderen/road-registry/commit/8233da57ce952691278151eb3fe2270249def663))
* Required environment variables on top level ([#1043](https://github.com/informatievlaanderen/road-registry/issues/1043)) ([bbcf73a](https://github.com/informatievlaanderen/road-registry/commit/bbcf73a7a9b496951f2ace01a5fd68cdd7723d72))
* Startup failure after last merge ([#1040](https://github.com/informatievlaanderen/road-registry/issues/1040)) ([b879830](https://github.com/informatievlaanderen/road-registry/commit/b8798306004e50908f7f59bbb88a69c8ba697697))
* Version bump ([#1035](https://github.com/informatievlaanderen/road-registry/issues/1035)) ([0d3d79b](https://github.com/informatievlaanderen/road-registry/commit/0d3d79bac34c999cd6b0dd6133f4f80c5c116f69))
* WR-757 ignore errors during CleanArchive, let validation handle it ([#1045](https://github.com/informatievlaanderen/road-registry/issues/1045)) ([e7016a5](https://github.com/informatievlaanderen/road-registry/commit/e7016a55103753fc1650ce6b3b188900f09061c9))
* WR-763 WR-695 FeatureCompare translation tweaks + add validation for TotPos>VanPos ([#1042](https://github.com/informatievlaanderen/road-registry/issues/1042)) ([e971a09](https://github.com/informatievlaanderen/road-registry/commit/e971a09345e53509ac011efa3d69bb36ce5fba6a))

## [3.52.1](https://github.com/informatievlaanderen/road-registry/compare/v3.52.0...v3.52.1) (2023-07-03)


### Bug Fixes

* WR-755 disable StreamStore prefetch jsondata ([#1032](https://github.com/informatievlaanderen/road-registry/issues/1032)) ([2a9afb7](https://github.com/informatievlaanderen/road-registry/commit/2a9afb7516ee052af55bc58194e57a3493935d09))

# [3.52.0](https://github.com/informatievlaanderen/road-registry/compare/v3.51.3...v3.52.0) (2023-07-03)


### Features

* WR-689 portaal ACM/IDM login ([#1031](https://github.com/informatievlaanderen/road-registry/issues/1031)) ([ef7ec71](https://github.com/informatievlaanderen/road-registry/commit/ef7ec7161bbe35215c09a46142b067673881d77d))

## [3.51.3](https://github.com/informatievlaanderen/road-registry/compare/v3.51.2...v3.51.3) (2023-06-30)


### Bug Fixes

* Email client should not send mails when not configured ([#1030](https://github.com/informatievlaanderen/road-registry/issues/1030)) ([29c291a](https://github.com/informatievlaanderen/road-registry/commit/29c291a2c3af7111f8fb87d03fd3ebb3bc647af5))

## [3.51.2](https://github.com/informatievlaanderen/road-registry/compare/v3.51.1...v3.51.2) (2023-06-26)


### Bug Fixes

* WR-703 add RoadSegmentPositionConverter + unit tests for SQS serialization/deserialization ([#1021](https://github.com/informatievlaanderen/road-registry/issues/1021)) ([8688b48](https://github.com/informatievlaanderen/road-registry/commit/8688b48ea36ff321e1558bf035ff8ede22796059))

## [3.51.1](https://github.com/informatievlaanderen/road-registry/compare/v3.51.0...v3.51.1) (2023-06-23)


### Bug Fixes

* WR-703 ignore empty arrays as input ([#1019](https://github.com/informatievlaanderen/road-registry/issues/1019)) ([21d11b9](https://github.com/informatievlaanderen/road-registry/commit/21d11b9103ea7580c418eeb31d74903d9ac5e121))
* WR-703 use RoadRegistryEnumDataTypeAttribute instead of EnumDataTypeAttribute to be able to use non-Enum types ([#1020](https://github.com/informatievlaanderen/road-registry/issues/1020)) ([d49b813](https://github.com/informatievlaanderen/road-registry/commit/d49b813e7f118ef9c98219b29a9061a37436bc6e))

# [3.51.0](https://github.com/informatievlaanderen/road-registry/compare/v3.50.1...v3.51.0) (2023-06-22)


### Bug Fixes

* WR-602 ensure SRID is filled in in projections ([#1017](https://github.com/informatievlaanderen/road-registry/issues/1017)) ([ad98724](https://github.com/informatievlaanderen/road-registry/commit/ad98724db041c757726819edeed4e6fe4172f069))
* WR-701 CPG file location in product zip ([#1018](https://github.com/informatievlaanderen/road-registry/issues/1018)) ([7f13442](https://github.com/informatievlaanderen/road-registry/commit/7f134425b3f9d29974467ce81105c32f245f5ce5))


### Features

* Add email client to send out error mails ([#1016](https://github.com/informatievlaanderen/road-registry/issues/1016)) ([e8837d2](https://github.com/informatievlaanderen/road-registry/commit/e8837d2f3ab7d8c27b598492ba624302481c3d1e))
* WR-724 use soft-delete for roadsegment in projections ([#1015](https://github.com/informatievlaanderen/road-registry/issues/1015)) ([ade342e](https://github.com/informatievlaanderen/road-registry/commit/ade342e4f421198832f0d4bdaa9016652aeca84d))

## [3.50.1](https://github.com/informatievlaanderen/road-registry/compare/v3.50.0...v3.50.1) (2023-06-20)


### Bug Fixes

* WR-262 store roadsegment EuropeanRoad/NationalRoad/NumberedRoad attributes as whole objects in snapshot ([#1014](https://github.com/informatievlaanderen/road-registry/issues/1014)) ([b53f0f3](https://github.com/informatievlaanderen/road-registry/commit/b53f0f3b54da6e0ca9cb035f5b4b7c315594ef0a))

# [3.50.0](https://github.com/informatievlaanderen/road-registry/compare/v3.49.2...v3.50.0) (2023-06-20)


### Bug Fixes

* ExtractRequest geometry conversion to MultiPolygon ([#1010](https://github.com/informatievlaanderen/road-registry/issues/1010)) ([3f7f9ea](https://github.com/informatievlaanderen/road-registry/commit/3f7f9ea47e68af2397dcf3ec3afb2d133496aff2))
* WR-698 add VersieNummer to get roadsegments endpoint ([#1009](https://github.com/informatievlaanderen/road-registry/issues/1009)) ([2927fba](https://github.com/informatievlaanderen/road-registry/commit/2927fba2ee944cd24dd3dd1285a2170cc2afc487))
* WR-742 identifier matching in route ([#1011](https://github.com/informatievlaanderen/road-registry/issues/1011)) ([65b77b1](https://github.com/informatievlaanderen/road-registry/commit/65b77b14713f96e3df5138caf5b73973f2dc9435))


### Features

* WR-701 add CPG files for each SHP file in extracts/products ([#1008](https://github.com/informatievlaanderen/road-registry/issues/1008)) ([3e93b3a](https://github.com/informatievlaanderen/road-registry/commit/3e93b3aaee26d5f91aa3c02580d7a5bf3417f9e6))
* WR-711 clean ziparchive before processing ([#1006](https://github.com/informatievlaanderen/road-registry/issues/1006)) ([870f86b](https://github.com/informatievlaanderen/road-registry/commit/870f86b73831d917841d09fa49bd63af4b6d2962))
* WR-740 extracthost wait for editorcontext projections to be up-to-date before processing extracts ([#1007](https://github.com/informatievlaanderen/road-registry/issues/1007)) ([6348b10](https://github.com/informatievlaanderen/road-registry/commit/6348b100d80cb7753db94e3a120cddaa943a40d9))

## [3.49.2](https://github.com/informatievlaanderen/road-registry/compare/v3.49.1...v3.49.2) (2023-06-14)


### Bug Fixes

* do not log ApiExceptions with StatusCode < 500 ([#1005](https://github.com/informatievlaanderen/road-registry/issues/1005)) ([e2e9643](https://github.com/informatievlaanderen/road-registry/commit/e2e964301de13af006b2786144c2318dfc0fc270))
* WR-739 manual close request endpoint ([#1004](https://github.com/informatievlaanderen/road-registry/issues/1004)) ([1e6e093](https://github.com/informatievlaanderen/road-registry/commit/1e6e093d48673554a0cefd710bced73e74848399))

## [3.49.1](https://github.com/informatievlaanderen/road-registry/compare/v3.49.0...v3.49.1) (2023-06-14)


### Bug Fixes

* consolidate projections behavior when deleting records ([#1002](https://github.com/informatievlaanderen/road-registry/issues/1002)) ([5058bfa](https://github.com/informatievlaanderen/road-registry/commit/5058bfae96dfd1a7b82181609d92dcfada6762f6))

# [3.49.0](https://github.com/informatievlaanderen/road-registry/compare/v3.48.0...v3.49.0) (2023-06-13)


### Bug Fixes

* WR-712 save original roadsegment ID when running FC ([#999](https://github.com/informatievlaanderen/road-registry/issues/999)) ([7d0e53d](https://github.com/informatievlaanderen/road-registry/commit/7d0e53d399dc29699dea74d5f0840c49cd92e24d))
* WR-738 don't fail when nationalroad projection tries to delete something that's already deleted ([#1001](https://github.com/informatievlaanderen/road-registry/issues/1001)) ([48fcd1d](https://github.com/informatievlaanderen/road-registry/commit/48fcd1dfa6368730973c8d353a0764823268cc92))


### Features

* Introduce Serilog sink for Slack channels ([#1000](https://github.com/informatievlaanderen/road-registry/issues/1000)) ([01f3875](https://github.com/informatievlaanderen/road-registry/commit/01f38753b98323c8b4a7458fc6448ece2f7fede4))

# [3.48.0](https://github.com/informatievlaanderen/road-registry/compare/v3.47.1...v3.48.0) (2023-06-12)


### Bug Fixes

* set RecordPositionThreshold to 1 for PositionStoreEventProcessor ([#995](https://github.com/informatievlaanderen/road-registry/issues/995)) ([0dd54d5](https://github.com/informatievlaanderen/road-registry/commit/0dd54d5a4e9c5fb9faf0baa2838e85a7507ea5f1))
* WFS roadnode projection use FindAsync instead of SingleAsync ([#997](https://github.com/informatievlaanderen/road-registry/issues/997)) ([a462d37](https://github.com/informatievlaanderen/road-registry/commit/a462d37cc9f72b18db66a1168116200d73284dc7))
* WR-732 change edit attributed endpoint request structure ([#994](https://github.com/informatievlaanderen/road-registry/issues/994)) ([52018b8](https://github.com/informatievlaanderen/road-registry/commit/52018b8d4d9f65908b9148b6684414a5a9ffd51b))


### Features

* WR-703 add new edit roadsegment endpoint for wegverharding/wegbreedte/aantalRijstroken ([#996](https://github.com/informatievlaanderen/road-registry/issues/996)) ([06ad19d](https://github.com/informatievlaanderen/road-registry/commit/06ad19d45a8c63701b9babd23fe31e3378b37543))

## [3.47.1](https://github.com/informatievlaanderen/road-registry/compare/v3.47.0...v3.47.1) (2023-06-07)


### Bug Fixes

* projections behavior when removing a removed entity + change projection RecordPositionThreshold to 1 ([#993](https://github.com/informatievlaanderen/road-registry/issues/993)) ([5d893e2](https://github.com/informatievlaanderen/road-registry/commit/5d893e23ef46213d167ecc7df05b0646cd1f9db5))

# [3.47.0](https://github.com/informatievlaanderen/road-registry/compare/v3.46.2...v3.47.0) (2023-06-06)


### Bug Fixes

* Maximum lenght error inside unit test fixture ([#992](https://github.com/informatievlaanderen/road-registry/issues/992)) ([d32eb29](https://github.com/informatievlaanderen/road-registry/commit/d32eb292bdaf3ba5b05ae8d9fb021594511d131a))
* WR-224 return validation errors for uploads endpoint instead of 409 ([#990](https://github.com/informatievlaanderen/road-registry/issues/990)) ([f0e05ef](https://github.com/informatievlaanderen/road-registry/commit/f0e05efe1586fcd86d7744c64a751bfb794d2dcc))


### Features

* Added cleanup task for roadnetwork extracts and now keeps track of when extracts are downloaded ([#991](https://github.com/informatievlaanderen/road-registry/issues/991)) ([5f1ab26](https://github.com/informatievlaanderen/road-registry/commit/5f1ab267d85c1df46cd2a2ccdc8b329b663e8fc2))

## [3.46.2](https://github.com/informatievlaanderen/road-registry/compare/v3.46.1...v3.46.2) (2023-06-05)


### Bug Fixes

* WR-244 Block incoming informative extract requests and close extract requests when upload change accepted ([#988](https://github.com/informatievlaanderen/road-registry/issues/988)) ([2c64341](https://github.com/informatievlaanderen/road-registry/commit/2c6434124ce744e069fa3c739cef596f8c0d4976))

## [3.46.1](https://github.com/informatievlaanderen/road-registry/compare/v3.46.0...v3.46.1) (2023-06-05)


### Bug Fixes

* Informative extract requests should be blocked ([#984](https://github.com/informatievlaanderen/road-registry/issues/984)) ([1651bf3](https://github.com/informatievlaanderen/road-registry/commit/1651bf3dcf440b4b7fe4bd1c12443376ea264e47))
* remove unused restsharp dependency ([#983](https://github.com/informatievlaanderen/road-registry/issues/983)) ([7ac1880](https://github.com/informatievlaanderen/road-registry/commit/7ac18806f84442a801d6a8adb1a78b78f11f7fe6))

# [3.46.0](https://github.com/informatievlaanderen/road-registry/compare/v3.45.1...v3.46.0) (2023-06-02)


### Features

* WR224 Extract or informative extract ([#982](https://github.com/informatievlaanderen/road-registry/issues/982)) ([db2280c](https://github.com/informatievlaanderen/road-registry/commit/db2280ccfec6678722c02146425cbc29ad2cfa60))

## [3.45.1](https://github.com/informatievlaanderen/road-registry/compare/v3.45.0...v3.45.1) (2023-06-02)


### Bug Fixes

* WR-258 use GRB basiskaart for municipalities ([#979](https://github.com/informatievlaanderen/road-registry/issues/979)) ([4ca03e2](https://github.com/informatievlaanderen/road-registry/commit/4ca03e225cc4b899d48f6331585f5712de56f92e))
* WR-725 WR-726 use GeoJSONMultiLineString and decorate swagger enum/required/notnull values ([#980](https://github.com/informatievlaanderen/road-registry/issues/980)) ([dfb0a13](https://github.com/informatievlaanderen/road-registry/commit/dfb0a132d206a4e2688120b41d2c5046db7d2fb2))

# [3.45.0](https://github.com/informatievlaanderen/road-registry/compare/v3.44.0...v3.45.0) (2023-05-30)


### Features

* WR-258 tab TransactionZones with map ([#978](https://github.com/informatievlaanderen/road-registry/issues/978)) ([c79689c](https://github.com/informatievlaanderen/road-registry/commit/c79689ccb58e3245fed42a2ca08def3f1698c1b7))

# [3.44.0](https://github.com/informatievlaanderen/road-registry/compare/v3.43.5...v3.44.0) (2023-05-30)


### Features

* Extract request should have upload expected and details endpoints WR-224 ([#976](https://github.com/informatievlaanderen/road-registry/issues/976)) ([9cc6446](https://github.com/informatievlaanderen/road-registry/commit/9cc6446722ec89089c369c0398b0ea9f562368aa))

## [3.43.5](https://github.com/informatievlaanderen/road-registry/compare/v3.43.4...v3.43.5) (2023-05-24)


### Bug Fixes

* WR-705 fix measure ordinates when reading SHP ([#974](https://github.com/informatievlaanderen/road-registry/issues/974)) ([b1f1265](https://github.com/informatievlaanderen/road-registry/commit/b1f1265aa99c4ee51425293449cd362a9c39c8c4))
* WR-715 upload through UI should read http status 202 succesful upload ([#975](https://github.com/informatievlaanderen/road-registry/issues/975)) ([bbfe4b6](https://github.com/informatievlaanderen/road-registry/commit/bbfe4b6af2343eee75524296ebe99df75db41c44))

## [3.43.4](https://github.com/informatievlaanderen/road-registry/compare/v3.43.3...v3.43.4) (2023-05-24)


### Bug Fixes

* WR-321 validate WKT on paste and when clicking next ([#973](https://github.com/informatievlaanderen/road-registry/issues/973)) ([4aee4c1](https://github.com/informatievlaanderen/road-registry/commit/4aee4c12ffe714ed20d4c7a6f7c16633d7243519))

## [3.43.3](https://github.com/informatievlaanderen/road-registry/compare/v3.43.2...v3.43.3) (2023-05-24)


### Bug Fixes

* WR-705 always fill in measure values when receiving geometries ([#972](https://github.com/informatievlaanderen/road-registry/issues/972)) ([d27ad76](https://github.com/informatievlaanderen/road-registry/commit/d27ad761841becd037da5bd04b5828913a1c2e7a))

## [3.43.2](https://github.com/informatievlaanderen/road-registry/compare/v3.43.1...v3.43.2) (2023-05-23)


### Bug Fixes

* Bugfix for incorrect json output ([#971](https://github.com/informatievlaanderen/road-registry/issues/971)) ([dcb2e48](https://github.com/informatievlaanderen/road-registry/commit/dcb2e48306127f15772d08b8de3233c9f3e54ab4))

## [3.43.1](https://github.com/informatievlaanderen/road-registry/compare/v3.43.0...v3.43.1) (2023-05-23)


### Bug Fixes

* WR-487 stop using nginx for API calls to backoffice-api or public-api ([#968](https://github.com/informatievlaanderen/road-registry/issues/968)) ([5112513](https://github.com/informatievlaanderen/road-registry/commit/5112513b041d124f6bd3295628c9e5e8c471bf77))
* WR-715 return UploadId for before-FC extracts upload ([#969](https://github.com/informatievlaanderen/road-registry/issues/969)) ([ca99eee](https://github.com/informatievlaanderen/road-registry/commit/ca99eeeb7a157cb61e78fd5fab448b13ba0687e0))

# [3.43.0](https://github.com/informatievlaanderen/road-registry/compare/v3.42.0...v3.43.0) (2023-05-17)


### Bug Fixes

* Next button not disabled when incorrect flow ([#964](https://github.com/informatievlaanderen/road-registry/issues/964)) ([9d5fd01](https://github.com/informatievlaanderen/road-registry/commit/9d5fd0158e9b4f8ad21b84f620c2dca43b83d4e9))
* ProblemCode.FromReason can deal with null value ([#963](https://github.com/informatievlaanderen/road-registry/issues/963)) ([44d0448](https://github.com/informatievlaanderen/road-registry/commit/44d0448387c4e691931c2c133dbe860519eaca66))
* Removed straggler csproj file ([#966](https://github.com/informatievlaanderen/road-registry/issues/966)) ([6f4c103](https://github.com/informatievlaanderen/road-registry/commit/6f4c1037d3c997634a4a55cd2dd024f8c9a37cb7))
* WR-713 register MessageHandler in RoadRegistryLambdaFunction ([#965](https://github.com/informatievlaanderen/road-registry/issues/965)) ([8134e05](https://github.com/informatievlaanderen/road-registry/commit/8134e052e31cca03380731a4b2fc9304b14aa7ef))
* WR-715 return 409 when doing multiple uploads for same extract ([#961](https://github.com/informatievlaanderen/road-registry/issues/961)) ([5328c65](https://github.com/informatievlaanderen/road-registry/commit/5328c659ccfd9ff7db47fb54376f14e55869ec31))


### Features

* Update paket dependencies ([#962](https://github.com/informatievlaanderen/road-registry/issues/962)) ([ae3ae1d](https://github.com/informatievlaanderen/road-registry/commit/ae3ae1d8a181362d28441e7a2e033fea79b2f19e))

# [3.42.0](https://github.com/informatievlaanderen/road-registry/compare/v3.41.2...v3.42.0) (2023-05-15)


### Features

* WR-431 WR-626 migrate FeatureCompare to RoadRegistry + docu text ([#959](https://github.com/informatievlaanderen/road-registry/issues/959)) ([87865bb](https://github.com/informatievlaanderen/road-registry/commit/87865bbab058c02b7329ed33547c065dee9f8591))

## [3.41.2](https://github.com/informatievlaanderen/road-registry/compare/v3.41.1...v3.41.2) (2023-05-10)


### Bug Fixes

* Maximum square kilometer area WR-321 ([#956](https://github.com/informatievlaanderen/road-registry/issues/956)) ([d31730c](https://github.com/informatievlaanderen/road-registry/commit/d31730c7678f9f27b15e798e6628c9cfef22c5cd))
* Minimal length for extract description ([#958](https://github.com/informatievlaanderen/road-registry/issues/958)) ([77a7dad](https://github.com/informatievlaanderen/road-registry/commit/77a7dad13bb592bd915555a822947abc217506af))

## [3.41.1](https://github.com/informatievlaanderen/road-registry/compare/v3.41.0...v3.41.1) (2023-05-03)


### Bug Fixes

* Road registry release pipeline ([#950](https://github.com/informatievlaanderen/road-registry/issues/950)) ([63acc0c](https://github.com/informatievlaanderen/road-registry/commit/63acc0c3894dcdcfa46c6e8e630b18d23b1371bc))
* Version bump for test with Release V2 ([#949](https://github.com/informatievlaanderen/road-registry/issues/949)) ([7a90f53](https://github.com/informatievlaanderen/road-registry/commit/7a90f53e90d0c37e7eba908262e35564439669d8))

# [3.41.0](https://github.com/informatievlaanderen/road-registry/compare/v3.40.3...v3.41.0) (2023-04-27)


### Bug Fixes

* AdminHost read SQS messages correctly ([#948](https://github.com/informatievlaanderen/road-registry/issues/948)) ([b5f6805](https://github.com/informatievlaanderen/road-registry/commit/b5f68059a62d52917f7db6687483bf2172e7ac55))


### Features

* Updated release pipeline ([#947](https://github.com/informatievlaanderen/road-registry/issues/947)) ([9991dcf](https://github.com/informatievlaanderen/road-registry/commit/9991dcf8719edd71984eafcd57bb7a3150af0da0))

## [3.40.3](https://github.com/informatievlaanderen/road-registry/compare/v3.40.2...v3.40.3) (2023-04-27)


### Bug Fixes

* add logging when no SQS message received ([#946](https://github.com/informatievlaanderen/road-registry/issues/946)) ([8f83e82](https://github.com/informatievlaanderen/road-registry/commit/8f83e825610bcebed34ed86978ed386102cb1ec4))

## [3.40.2](https://github.com/informatievlaanderen/road-registry/compare/v3.40.1...v3.40.2) (2023-04-26)


### Bug Fixes

* release pipeline, add step to delete /opt/hostedtoolcache ([#944](https://github.com/informatievlaanderen/road-registry/issues/944)) ([49f39ef](https://github.com/informatievlaanderen/road-registry/commit/49f39ef50070dd896bb71c2e37ef8d4c44bbf306))

## [3.40.1](https://github.com/informatievlaanderen/road-registry/compare/v3.40.0...v3.40.1) (2023-04-26)


### Bug Fixes

* Incorrect validation of outlined road segment from dbase record ([#943](https://github.com/informatievlaanderen/road-registry/issues/943)) ([601069d](https://github.com/informatievlaanderen/road-registry/commit/601069d852ba6d031cfdf152a9c748511a8a24b6))

# [3.40.0](https://github.com/informatievlaanderen/road-registry/compare/v3.39.7...v3.40.0) (2023-04-26)


### Bug Fixes

* move and rename connectors ([#942](https://github.com/informatievlaanderen/road-registry/issues/942)) ([91fd9e2](https://github.com/informatievlaanderen/road-registry/commit/91fd9e268a037e9765d23f2e873fd9832a6e5521))


### Features

* WR-684 add AdminHost for long running admin operations ([#941](https://github.com/informatievlaanderen/road-registry/issues/941)) ([c6230d4](https://github.com/informatievlaanderen/road-registry/commit/c6230d4ae87545036a39a79d4cc227fcabce6a6b))

## [3.39.7](https://github.com/informatievlaanderen/road-registry/compare/v3.39.6...v3.39.7) (2023-04-24)


### Bug Fixes

* MessageGroupId for roadsegment edit endpoints ([#938](https://github.com/informatievlaanderen/road-registry/issues/938)) ([310d202](https://github.com/informatievlaanderen/road-registry/commit/310d202452513980ece42a6988c457bf6226194e))
* WR-658 extracts upload ([#937](https://github.com/informatievlaanderen/road-registry/issues/937)) ([833e2d8](https://github.com/informatievlaanderen/road-registry/commit/833e2d8d4f88925877b90c19951504e02641a815))

## [3.39.6](https://github.com/informatievlaanderen/road-registry/compare/v3.39.5...v3.39.6) (2023-04-20)


### Bug Fixes

* WR-684 move long running Lambdas to API+CommandHost ([#935](https://github.com/informatievlaanderen/road-registry/issues/935)) ([fc24ea6](https://github.com/informatievlaanderen/road-registry/commit/fc24ea6aef6a79e5d22a08f75f1436cc8797dd0d))

## [3.39.5](https://github.com/informatievlaanderen/road-registry/compare/v3.39.4...v3.39.5) (2023-04-19)


### Bug Fixes

* Invalid JSON error from basic model binding issues ([#934](https://github.com/informatievlaanderen/road-registry/issues/934)) ([6288412](https://github.com/informatievlaanderen/road-registry/commit/6288412b49ea279f3297bee33937d3243fb917a6))

## [3.39.4](https://github.com/informatievlaanderen/road-registry/compare/v3.39.3...v3.39.4) (2023-04-19)


### Bug Fixes

* enable snapshot lambda in release pipeline ([#933](https://github.com/informatievlaanderen/road-registry/issues/933)) ([b50a7b9](https://github.com/informatievlaanderen/road-registry/commit/b50a7b998f2311408a9d9a24d4b54821db626121))

## [3.39.3](https://github.com/informatievlaanderen/road-registry/compare/v3.39.2...v3.39.3) (2023-04-18)


### Bug Fixes

* Update ticketing service capabilities ([#932](https://github.com/informatievlaanderen/road-registry/issues/932)) ([f649cc5](https://github.com/informatievlaanderen/road-registry/commit/f649cc59f9353de29cb96939c12be7172a9fd83d))

## [3.39.2](https://github.com/informatievlaanderen/road-registry/compare/v3.39.1...v3.39.2) (2023-04-18)


### Bug Fixes

* WR-643 set roadnode version correctly ([#931](https://github.com/informatievlaanderen/road-registry/issues/931)) ([bd673aa](https://github.com/informatievlaanderen/road-registry/commit/bd673aa8f3c0fbe550e67d10aed32937b48dde0d))

## [3.39.1](https://github.com/informatievlaanderen/road-registry/compare/v3.39.0...v3.39.1) (2023-04-17)


### Bug Fixes

* endpoint documentation ([#926](https://github.com/informatievlaanderen/road-registry/issues/926)) ([9a83e06](https://github.com/informatievlaanderen/road-registry/commit/9a83e0639618407fd1a735693bdd1161965a3679))
* Updated change attribute endpoint ([#927](https://github.com/informatievlaanderen/road-registry/issues/927)) ([8526dbb](https://github.com/informatievlaanderen/road-registry/commit/8526dbbede4ac91118d64cc49ca6ee326c402eb5))

# [3.39.0](https://github.com/informatievlaanderen/road-registry/compare/v3.38.8...v3.39.0) (2023-04-14)


### Features

* WR-525 WR-526 roadsegment outlined change geometry endpoint ([#923](https://github.com/informatievlaanderen/road-registry/issues/923)) ([efeaa32](https://github.com/informatievlaanderen/road-registry/commit/efeaa32d54ff9563aeff520fa52d51dd45205a12))

## [3.38.8](https://github.com/informatievlaanderen/road-registry/compare/v3.38.7...v3.38.8) (2023-04-12)


### Bug Fixes

* WR-505 remove GeometryDrawMethod 'Measured_according_to_GRB_specifications' as a valid value ([#922](https://github.com/informatievlaanderen/road-registry/issues/922)) ([d9918d2](https://github.com/informatievlaanderen/road-registry/commit/d9918d287685fd94cd909b6a09f7d356b7717a3f))
* WR-676 Updated scopes for link/unlink streetname ([#924](https://github.com/informatievlaanderen/road-registry/issues/924)) ([32d68eb](https://github.com/informatievlaanderen/road-registry/commit/32d68eb8dd5da75d3cfa52ac81f7417cc92e84ab))

## [3.38.7](https://github.com/informatievlaanderen/road-registry/compare/v3.38.6...v3.38.7) (2023-04-07)


### Bug Fixes

* WR-671 connection string ([#921](https://github.com/informatievlaanderen/road-registry/issues/921)) ([b7f12ad](https://github.com/informatievlaanderen/road-registry/commit/b7f12ad70eea47b374b50542a64c8160eaa27114))

## [3.38.6](https://github.com/informatievlaanderen/road-registry/compare/v3.38.5...v3.38.6) (2023-04-07)


### Bug Fixes

* WR-671 convert wfs from cron job to continuous service ([#920](https://github.com/informatievlaanderen/road-registry/issues/920)) ([8351452](https://github.com/informatievlaanderen/road-registry/commit/8351452edb11447504a1de8b5f2b223da2caf3a5))

## [3.38.5](https://github.com/informatievlaanderen/road-registry/compare/v3.38.4...v3.38.5) (2023-04-06)


### Bug Fixes

* get roadsegment response example ([#919](https://github.com/informatievlaanderen/road-registry/issues/919)) ([6196b14](https://github.com/informatievlaanderen/road-registry/commit/6196b14a557316712b65d1e1663386db09cddae5))

## [3.38.4](https://github.com/informatievlaanderen/road-registry/compare/v3.38.3...v3.38.4) (2023-04-06)


### Bug Fixes

* WR-624 Updated road segment detail response with additional data ([#917](https://github.com/informatievlaanderen/road-registry/issues/917)) ([e88ce28](https://github.com/informatievlaanderen/road-registry/commit/e88ce2819f8a9c90a035ffe22dc9a4fba748b9ca))

## [3.38.3](https://github.com/informatievlaanderen/road-registry/compare/v3.38.2...v3.38.3) (2023-04-05)


### Bug Fixes

* Validation behaviour and messages for WR-651, WR-652, WR-653, WR-654, WR-656, WR-657 ([#915](https://github.com/informatievlaanderen/road-registry/issues/915)) ([29521f8](https://github.com/informatievlaanderen/road-registry/commit/29521f85038be58649ff36a4b6a71d0e4cb8683a))
* WR-631 wms projectionhost SyndicationContext disposed bug ([e199f6b](https://github.com/informatievlaanderen/road-registry/commit/e199f6b50d27dfba76454bbb3f9852ab14fbd27b))

## [3.38.2](https://github.com/informatievlaanderen/road-registry/compare/v3.38.1...v3.38.2) (2023-04-04)


### Bug Fixes

* make wms projection hostedservice ([17a36af](https://github.com/informatievlaanderen/road-registry/commit/17a36af01b11e069a16767b421b3296eb842a601))
* run containers as non-root ([#913](https://github.com/informatievlaanderen/road-registry/issues/913)) ([c3bcf16](https://github.com/informatievlaanderen/road-registry/commit/c3bcf1613d16653c4d0afd62a1ccee6e23f0c96e))
* WR-606 Adapt NotFound for removed streetname objects ([#914](https://github.com/informatievlaanderen/road-registry/issues/914)) ([5994a26](https://github.com/informatievlaanderen/road-registry/commit/5994a26e4abe3438c5fb972790498d3c11cb6376))

## [3.38.1](https://github.com/informatievlaanderen/road-registry/compare/v3.38.0...v3.38.1) (2023-04-03)


### Bug Fixes

* WR-646 change extract integration data logic ([#912](https://github.com/informatievlaanderen/road-registry/issues/912)) ([90854a2](https://github.com/informatievlaanderen/road-registry/commit/90854a215c8d6cd5205e08f814b53e857efcb86d))

# [3.38.0](https://github.com/informatievlaanderen/road-registry/compare/v3.37.1...v3.38.0) (2023-04-03)


### Bug Fixes

* WR-650 validation for RoadSegmentId ([#907](https://github.com/informatievlaanderen/road-registry/issues/907)) ([45e302a](https://github.com/informatievlaanderen/road-registry/commit/45e302a6ffaa2e10d51211c89d09668a2759af26))


### Features

* WR-561 add authentication through ACM/IDM for edit endpoints ([#906](https://github.com/informatievlaanderen/road-registry/issues/906)) ([a577bef](https://github.com/informatievlaanderen/road-registry/commit/a577bef7ea3a2f45b175ea636c7393bc6d1a7d4a))
* WR-647 road segment attributes edit endpoint ([#905](https://github.com/informatievlaanderen/road-registry/issues/905)) ([14a8e43](https://github.com/informatievlaanderen/road-registry/commit/14a8e439f603ba184cef61db095bec52a89d42ee))

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
