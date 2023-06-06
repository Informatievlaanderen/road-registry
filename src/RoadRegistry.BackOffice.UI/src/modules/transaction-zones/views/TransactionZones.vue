<template>
  <div>
    <wr-h2>Bijwerkingszones</wr-h2>
    <wr-h3>Bijwerkingskaart</wr-h3>

    <vl-grid mod-stacked>
      <vl-column>
        <vl-checkbox v-model="showRoadRegistryLayer">Toon het Wegenregister</vl-checkbox>

        <vl-ol-map id="map" ref="map" mod-boxed map-zoomable map-expandable>
          <vl-map-tile-layer>
            <vl-map-tile-wms-source url="https://geo.api.vlaanderen.be/GRB-basiskaart/wms" />
          </vl-map-tile-layer>

          <!-- <vl-map-tile-layer>
            <vl-map-tile-wms-source url="https://geo.api.vlaanderen.be/Wegenregister/wms" />
          </vl-map-tile-layer> -->

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
import TileLayer from "ol/layer/tile";
import TileWMS from "ol/source/tilewms";
import { WR_ENV, API_ENDPOINT, API_OLDENDPOINT } from "@/environment";

const usePublicApi = WR_ENV !== "development";
const geoJsonBaseUrl = usePublicApi
  ? `${trimEnd(API_ENDPOINT, "/")}/v1/wegen/extract`
  : `${trimEnd(API_OLDENDPOINT, "/")}/v1/extracts`;

export default Vue.extend({
  data() {
    return {
      showRoadRegistryLayer: false,
      roadRegistryLayer: new TileLayer({
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
      }),
    };
  },
  computed: {
    olMap() {
      return (this.$refs.map as any).olMap;
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
      if (this.showRoadRegistryLayer) {
        this.olMap.addLayer(this.roadRegistryLayer);
      } else {
        this.olMap.removeLayer(this.roadRegistryLayer);
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
