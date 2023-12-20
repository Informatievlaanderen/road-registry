<template>
  <div>
    <wr-h2>Bijwerkingszones</wr-h2>
    <wr-h3>Bijwerkingskaart</wr-h3>

    <vl-grid mod-stacked>
      <vl-column>
        <vl-checkbox v-model="showRoadRegistryLayer">Toon het Wegenregister</vl-checkbox>

        <vl-ol-map ref="map" mod-boxed map-zoomable map-expandable>
          <vl-map-tile-layer>
            <vl-map-tile-wms-source url="https://geo.api.vlaanderen.be/GRB-basiskaart/wms" />
          </vl-map-tile-layer>

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
      </vl-column>
    </vl-grid>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { trimEnd } from "lodash";
import Map from "ol/Map";
import Tile from "ol/layer/Tile";
import TileWMS from "ol/source/TileWMS";
import { WR_ENV, API_ENDPOINT, API_OLDENDPOINT } from "@/environment";

const usePublicApi = WR_ENV !== "development";
const geoJsonBaseUrl = usePublicApi
  ? `${trimEnd(API_ENDPOINT, "/")}/v1/wegen/extract`
  : `${trimEnd(API_OLDENDPOINT, "/")}/v1/extracts`;

export default Vue.extend({
  data() {
    return {
      showRoadRegistryLayer: false,
      roadRegistryLayer: null as any,
    };
  },
  computed: {
    olMap(): Map {
      return (this.$refs.map as any).olMap as Map;
    },
    transactionZonesGeoJsonUrl() {
      return `${geoJsonBaseUrl}/transactionzones.geojson`;
    },
    overlappingTransactionZonesGeoJsonUrl() {
      return `${geoJsonBaseUrl}/overlappingtransactionzones.geojson`;
    },
  },
  watch: {
    showRoadRegistryLayer() {
      const olMap = this.olMap;

      if (this.showRoadRegistryLayer) {
        this.roadRegistryLayer = new Tile({
          source: new TileWMS({
            url: "https://geo.api.vlaanderen.be/Wegenregister/wms",
            params: {
              VERSION: "1.3.0",
              SERVICE: "WMS",
              REQUEST: "GetMap",
              FORMAT: "image/png",
              TRANSPARENT: "true",
              LAYERS: "AUTOSWEG",
              CRS: "EPSG:3857",
            },
            serverType: "geoserver",
          }),
        });
        olMap.addLayer(this.roadRegistryLayer);
      } else {
        olMap.removeLayer(this.roadRegistryLayer);
        this.roadRegistryLayer = null;
      }
    },
  },
  methods: {
    onSelectTransactionZone() {
      //console.log("onSelectTransactionZone", arguments);
    },
  },
  mounted() {},
});
</script>

<style lang="scss"></style>
