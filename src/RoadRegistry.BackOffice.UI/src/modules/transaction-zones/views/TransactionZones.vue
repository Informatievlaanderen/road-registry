<template>
  <div>
    <wr-h2>Bijwerkingszones</wr-h2>
    <wr-h3>Bijwerkingskaart</wr-h3>

    <vl-grid mod-stacked>
      <vl-column>
        <!-- <vl-checkbox v-model="showRoadRegistryLayer">Toon het Wegenregister</vl-checkbox> -->
        <vl-checkbox v-model="showGrbLayer">Toon GRB basiskaart</vl-checkbox>

        <template v-if="showGrbLayer">
          <vl-ol-map v-if="renderMap" mod-boxed map-zoomable map-expandable>
            <vl-map-tile-layer>
              <vl-map-tile-wms-source url="https://geo.api.vlaanderen.be/GRB-basiskaart/wms" />
            </vl-map-tile-layer>

            <!-- <vl-map-tile-layer>
              <vl-map-tile-wms-source url="https://geo.api.vlaanderen.be/Wegenregister/wms" />
            </vl-map-tile-layer> -->

            <vl-map-vector-layer>
              <vl-map-vector-source :url="municipalitiesGeoJsonUrl" />
              <vl-map-icon-style color="rgba(255, 255, 255, 0.5)" color-stroke="rgba(107, 106, 107, 1)" />
            </vl-map-vector-layer>

            <vl-map-vector-layer>
              <vl-map-vector-source :url="transactionZonesGeoJsonUrl" />
              <vl-map-icon-style color="rgba(183, 171, 31, 1)" color-stroke="rgba(183, 171, 31, 1)" />
              <!-- <vl-map-select-interaction @select="onSelectTransactionZone">
                <vl-map-icon-style mod-highlight />
              </vl-map-select-interaction> -->
            </vl-map-vector-layer>

            <vl-map-vector-layer>
              <vl-map-vector-source :url="overlappingTransactionZonesGeoJsonUrl" />
              <vl-map-icon-style color="rgba(230, 49, 31, 1)" color-stroke="rgba(230, 49, 31, 1)" />
            </vl-map-vector-layer>
          </vl-ol-map>
        </template>
        <template v-else>
          <vl-ol-map v-if="renderMap" mod-boxed map-zoomable map-expandable>
            <vl-map-vector-layer>
              <vl-map-vector-source :url="municipalitiesGeoJsonUrl" />
              <vl-map-icon-style color="rgba(255, 255, 255, 0.5)" color-stroke="rgba(107, 106, 107, 1)" />
            </vl-map-vector-layer>

            <vl-map-vector-layer>
              <vl-map-vector-source :url="transactionZonesGeoJsonUrl" />
              <vl-map-icon-style color="rgba(183, 171, 31, 1)" color-stroke="rgba(183, 171, 31, 1)" />
            </vl-map-vector-layer>

            <vl-map-vector-layer>
              <vl-map-vector-source :url="overlappingTransactionZonesGeoJsonUrl" />
              <vl-map-icon-style color="rgba(230, 49, 31, 1)" color-stroke="rgba(230, 49, 31, 1)" />
            </vl-map-vector-layer>
          </vl-ol-map>
        </template>
      </vl-column>
    </vl-grid>
  </div>
</template>

<script lang="ts">
//TODO-rik transactionZonesGeoJsonUrl: met beschrijving zichtbaar als label, tekstkleur van de labels: zwart -- #000000 (R0 G0 B0)
//TODO-rik gemeentenamen zichtbaar als labels, zonder overlap met labels (beschrijvingen) van extractaanvragen

import Vue from "vue";
// import TileLayer from "ol/layer/tile.js";
// import TileWMS from "ol/source/tilewms.js";
import { trimEnd } from "lodash";
import { API_OLDENDPOINT } from "@/environment";

const backofficeApiEndpoint = trimEnd(API_OLDENDPOINT, "/");

export default Vue.extend({
  data() {
    return {
      showRoadRegistryLayer: false,
      showGrbLayer: false,
      roadRegistryLayerId: 0,
      roadRegistryWmsLayers: [] as Array<string>,
      renderMap: true,
    };
  },
  computed: {
    olMap() {
      return (this.$refs.map as any).olMap;
    },
    municipalitiesGeoJsonUrl() {
      return `${backofficeApiEndpoint}/v1/extracts/municipalities.geojson`;
    },
    transactionZonesGeoJsonUrl() {
      return `${backofficeApiEndpoint}/v1/extracts/transactionzones.geojson`;
    },
    overlappingTransactionZonesGeoJsonUrl() {
      return `${backofficeApiEndpoint}/v1/extracts/overlappingtransactionzones.geojson`;
    },
  },
  watch: {
    showRoadRegistryLayer() {
      this.rebuildRoadRegistryLayer();
    },
    async showGrbLayer() {
      this.renderMap = false;
      await this.$nextTick();
      this.renderMap = true;
    },
  },
  methods: {
    getLayerCapabilities(url: string): any {
      return fetch(`${url}?request=getcapabilities`);
    },
    async rebuildRoadRegistryLayer() {
      // let layers = this.olMap.getLayers().array_;
      // let roadRegistryLayer = layers.find((x: any) => x.ol_uid === this.roadRegistryLayerId);
      // if (this.showRoadRegistryLayer) {
      //   if (roadRegistryLayer) {
      //     return;
      //   }
      //   const url = "https://geo.api.vlaanderen.be/Wegenregister/wms";
      //   if (!this.roadRegistryWmsLayers.length) {
      //     let capabilities = await this.getLayerCapabilities(url);
      //     console.log("Layer capabilities", url, capabilities);
      //     //TODO-rik eens CORS in orde is
      //     // if (capabilities.Capability.Layer.Name) {
      //     //   this.roadRegistryWmsLayers = [capabilities.Capability.Layer.Name];
      //     // } else {
      //     //   this.roadRegistryWmsLayers = capabilities.Capability.Layer.Layer.map(function (layer: any) {
      //     //     return layer.Name;
      //     //   });
      //     // }
      //   }
      //   roadRegistryLayer = new TileLayer({
      //     source: new TileWMS({
      //       url,
      //       params: {
      //         VERSION: "1.3.0",
      //         SERVICE: "WMS",
      //         REQUEST: "GetMap",
      //         FORMAT: "image/png",
      //         TRANSPARENT: "true",
      //         LAYERS: this.roadRegistryWmsLayers.join(","),
      //         CRS: "EPSG:31370",
      //       },
      //       serverType: "geoserver",
      //     }),
      //   });
      //   console.log("RoadRegistryLayer", roadRegistryLayer);
      //   this.olMap.addLayer(roadRegistryLayer);
      //   this.roadRegistryLayerId = roadRegistryLayer.ol_uid;
      // } else {
      //   if (roadRegistryLayer) {
      //     this.olMap.removeLayer(roadRegistryLayer);
      //   }
      // }
    },
    onSelectTransactionZone() {
      //console.log("onSelectTransactionZone", arguments);
    },
  },
  mounted() {},
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
