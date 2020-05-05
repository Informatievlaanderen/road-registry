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
