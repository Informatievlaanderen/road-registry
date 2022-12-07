# road-registry

## Project setup
```
npm install
```

### Compiles and hot-reloads for development
```
npm run serve
```

### Compiles and minifies for production
```
npm run build
```

### Lints and fixes files
```
npm run lint
```

### Customize configuration
See [Configuration Reference](https://cli.vuejs.org/config/).


## Create build and run locally in Docker
### TST
```
npm run build-tst
docker build . -t road-registry-backoffice-ui-tst:1.0.0 --build-arg build_number=1.0.0
docker run --env API_ENDPOINT=https://api.basisregisters.dev-vlaanderen.be/ --env API_OLDENDPOINT=https://backoffice-api.wegen.test-vlaanderen.be/ --env FeatureToggles__UseFeatureCompare=false -p 127.0.0.1:5010:10007/tcp road-registry-backoffice-ui-tst:1.0.0
start http://127.0.0.1:5010
```

### STG
```
npm run build-stg
docker build . -t road-registry-backoffice-ui-stg:1.0.0 --build-arg build_number=1.0.0
docker run --env API_ENDPOINT=https://api.basisregisters.staging-vlaanderen.be/ --env API_OLDENDPOINT=https://backoffice-api.wegen.dev-vlaanderen.be/ --env FeatureToggles__UseFeatureCompare=true -p 127.0.0.1:5010:10007/tcp road-registry-backoffice-ui-stg:1.0.0
start http://127.0.0.1:5010
```

### PRD
```
npm run build-prd
docker build . -t road-registry-backoffice-ui-prd:1.0.0 --build-arg build_number=1.0.0
docker run --env API_ENDPOINT=https://api.basisregisters.vlaanderen.be/ --env API_OLDENDPOINT=https://backoffice-api.wegen.vlaanderen.be/ --env FeatureToggles__UseFeatureCompare=true -p 127.0.0.1:5010:10007/tcp road-registry-backoffice-ui-prd:1.0.0
start http://127.0.0.1:5010
```