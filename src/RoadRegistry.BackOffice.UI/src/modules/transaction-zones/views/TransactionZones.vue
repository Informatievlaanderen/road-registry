<template>
  <div>
    <wr-h2>Bijwerkingszones</wr-h2>
    <wr-h3>Bijwerkingskaart</wr-h3>

    <vl-grid mod-stacked>
      <vl-column>
        <vl-checkbox v-model="showRoadRegistryLayer">Toon het Wegenregister</vl-checkbox>

        <vl-ol-map ref="map" mod-boxed map-zoomable>
          <vl-map-vector-layer>
            <vl-map-vector-source :url="overlappingTransactionZonesGeoJsonUrl" />
            <vl-map-icon-style color="rgba(230, 49, 31, 1)" color-stroke="rgba(183, 171, 31, 1)" />
          </vl-map-vector-layer>
          
          <vl-map-vector-layer>
            <vl-map-vector-source :url="transactionZonesGeoJsonUrl" />
            <!-- TODO-rik met beschrijving zichtbaar als label, tekstkleur van de labels: zwart -- #000000 (R0 G0 B0)  -->
            <vl-map-icon-style color="rgba(183, 171, 31, 1)" color-stroke="rgba(183, 171, 31, 1)" />
          </vl-map-vector-layer>

          <vl-map-tile-layer>
            <vl-map-tile-wms-source url="https://geo.api.beta-vlaanderen.be/Wegenregister/wms" />
            <!-- <vl-map-tile-wms-source url="http://localhost:10002/v1/information/wms" /> -->
          </vl-map-tile-layer>
          
          <vl-map-vector-layer>
            <vl-map-vector-source :url="municipalitiesGeoJsonUrl" />
            <!-- TODO-rik gemeentenamen zichtbaar als labels, zonder overlap met labels (beschrijvingen) van extractaanvragen -->
            <vl-map-icon-style color="rgba(255, 255, 255, 0.5)" color-stroke="rgba(107, 106, 107, 1)" />
          </vl-map-vector-layer>

          <vl-map-tile-layer>
            <vl-map-tile-wms-source url="https://geo.api.vlaanderen.be/GRB-basiskaart/wms" />
          </vl-map-tile-layer>
        </vl-ol-map>
      </vl-column>
    </vl-grid>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
//import * as ol from 'ol';
//import Map from 'ol/map.js';
// import OSM from 'ol/source/osm.js';
import TileLayer from "ol/layer/tile.js";
import TileWMS from "ol/source/tilewms.js";

//const { TileLayer, TileWMS } = require("@govflanders/vl-ui-vue-components");
// const TileLayer = require("ol/layer/tile");
// const TileWMS = require("ol/source/tilewms");

//import View from 'ol/view.js';
import { trimEnd } from "lodash";
import { API_OLDENDPOINT } from "@/environment";
const backofficeApiEndpoint = trimEnd(API_OLDENDPOINT, "/");

export default Vue.extend({
  data() {
    return {
      showRoadRegistryLayer: false,
      roadRegistryLayerId: 0,
      //https://geo.api.vlaanderen.be/Wegenregister/wms?SERVICE=WMS&VERSION=1.1.1&REQUEST=GetCapabilities
      roadRegistryWmsLayers: [] as Array<string>,
    };
  },
  computed: {
    olMap() {
      return (this.$refs.map as any).olMap;
    },
    municipalitiesGeoJsonUrl() {
      return `${backofficeApiEndpoint}/v1/information/municipalities.geojson`;
    },
    transactionZonesGeoJsonUrl() {
      return `${backofficeApiEndpoint}/v1/information/transactionzones.geojson`;
    },
    overlappingTransactionZonesGeoJsonUrl() {
      return `${backofficeApiEndpoint}/v1/information/overlappingtransactionzones.geojson`;
    },
  },
  watch: {
    showRoadRegistryLayer() {
      this.rebuildRoadRegistryLayer();
    },
  },
  methods: {
    getLayerCapabilities(url: string): any {
      return fetch(`${url}?request=getcapabilities`);
    },
    async rebuildRoadRegistryLayer() {
      let layers = this.olMap.getLayers().array_;
      let roadRegistryLayer = layers.find((x: any) => x.ol_uid === this.roadRegistryLayerId);

      if (this.showRoadRegistryLayer) {
        if (roadRegistryLayer) {
          return;
        }

        const url = "https://geo.api.vlaanderen.be/Wegenregister/wms";

        if (!this.roadRegistryWmsLayers.length) {
          let capabilities = await this.getLayerCapabilities(url);
          console.log("Layer capabilities", url, capabilities);
          //TODO-rik eens CORS in orde is
          // if (capabilities.Capability.Layer.Name) {
          //   this.roadRegistryWmsLayers = [capabilities.Capability.Layer.Name];
          // } else {
          //   this.roadRegistryWmsLayers = capabilities.Capability.Layer.Layer.map(function (layer: any) {
          //     return layer.Name;
          //   });
          // }
        }

        roadRegistryLayer = new TileLayer({
          source: new TileWMS({
            url,
            params: {
              VERSION: "1.3.0",
              SERVICE: "WMS",
              REQUEST: "GetMap",
              FORMAT: "image/png",
              TRANSPARENT: "true",
              LAYERS: this.roadRegistryWmsLayers.join(","),
              CRS: "EPSG:31370",
            },
            serverType: "geoserver",
          }),
        });
        console.log("RoadRegistryLayer", roadRegistryLayer);
        this.olMap.addLayer(roadRegistryLayer);
        this.roadRegistryLayerId = roadRegistryLayer.ol_uid;
      } else {
        if (roadRegistryLayer) {
          this.olMap.removeLayer(roadRegistryLayer);
        }
      }
    },
  },
  mounted() {
    // const map = new Map({
    //   target: 'map',
    //   layers: [
    //   new TileLayer({
    //       source: new TileWMS({
    //         url: "https://geoservices.informatievlaanderen.be/raadpleegdiensten/GRB-basiskaart/wms",
    //         params: {
    //           VERSION: "1.3.0",
    //           SERVICE: "WMS",
    //           REQUEST: "GetMap",
    //           FORMAT: "image/png",
    //           TRANSPARENT: "true",
    //           LAYERS: "GRB_BSK",
    //           CRS: "EPSG:31370",
    //         },
    //         serverType: "geoserver",
    //       }),
    //     }),
    //   ],
    //   view: new View({
    //     center: [0, 0],
    //     zoom: 2,
    //   }),
    // });
    // const OSMLayer = new TileLayer({
    //   source: new OSM(),
    // });
    //this.olMap.addLayer(OSMLayer);
    // const GRBLayer = new TileLayer({
    //   source: new TileWMS({
    //     url: "https://geoservices.informatievlaanderen.be/raadpleegdiensten/GRB-basiskaart/wms",
    //     params: {
    //       VERSION: "1.3.0",
    //       SERVICE: "WMS",
    //       REQUEST: "GetMap",
    //       FORMAT: "image/png",
    //       TRANSPARENT: "true",
    //       LAYERS: "GRB_BSK",
    //       CRS: "EPSG:31370",
    //     },
    //     serverType: "geoserver",
    //   }),
    // });
    // console.log("GRBLayer", GRBLayer);
    // this.olMap.addLayer(GRBLayer);
  },
});
</script>

<style lang="scss">
.vl-ol-map--boxed {
  border: 1px #cbd2da solid;
}

.vl-ol-map {
  position: relative;
  width: 100%;
  height: 100%;
  min-height: 450px;
  overflow: hidden;
  display: flex;
  font-size: 1.6rem;
}

.vl-ol-map__body {
  display: flex;
  flex: 1;
  height: 100%;
  overflow: hidden;
  position: relative;
}

.vl-ol-map__content {
  position: relative;
  width: 100%;
  flex: 1;
}

.vl-ol-map__map {
  width: 100%;
  height: 100%;
  top: 0;
  bottom: 0;
  left: 0;
  right: 0;
  position: absolute;
  background-color: #f7f9fc;
}

.vl-ol-map__view-tools {
  right: 2.5rem;
  bottom: 2.5rem;
}

.vl-ol-map__view-tools,
.vl-ol-map__action-tools,
.vl-ol-map__search {
  position: absolute;
  z-index: 10;
}

.vl-ol-map-tools-bar--vertical {
  display: flex;
  flex-direction: column;
}

.vl-ol-map-tools-bar--vertical .vl-ol-map-tools-action-group:not(:last-child) {
  margin-right: 0;
  margin-bottom: 1.5rem;
}

.vl-ol-map-tools-action {
  border-radius: 0;
  appearance: none;
  -webkit-appearance: none;
  border: 0;
  background-color: transparent;
  padding: 0;
  display: block;
  width: 3.5rem;
  height: 3.5rem;
  background-color: #fff;
  border: 0;
  border-radius: 2px;
  font-size: 16px;
  line-height: 18px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 0px 0.3rem 0.3rem 0px;
  color: #05c;
  background-color: #fff;
  border: none;
  box-shadow: none;
}

.vl-ol-map-tools-action-group--vertical {
  flex-direction: column;
}

.vl-ol-map-tools-action-group {
  display: flex;
  background: #fff;
  border-radius: 2px;
  box-shadow: 0px 2px 12px rgba(106, 118, 134, 0.35);
}

.vl-ol-map-tools-action-group .vl-ol-map-tools-action {
  border-radius: 0;
}

.vl-ol-map-tools-action__text {
  position: absolute !important;
  top: 0;
  left: 0;
  width: 1px;
  height: 1px;
  margin: -1px;
  padding: 0;
  border: 0;
  overflow: hidden;
  clip: rect(1px, 1px, 1px, 1px);
}
</style>
