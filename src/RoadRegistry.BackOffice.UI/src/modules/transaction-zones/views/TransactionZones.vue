<template>
  <div>
    <wr-h2>Bijwerkingszones</wr-h2>
    <wr-h3>Bijwerkingskaart</wr-h3>

    <vl-grid mod-stacked>
      <vl-column>
        <vl-ol-map ref="map" mod-boxed map-zoomable map-expandable>
          <vl-ol-map-tile-layer>
            <vl-ol-map-tile-wms-source
              url="https://geo.api.vlaanderen.be/GRB-basiskaart/wms"
            />
          </vl-ol-map-tile-layer>

          <vl-ol-map-tile-layer :opacity="0.8">
            <vl-ol-map-tile-wms-source
              :url="roadRegistryWmsUrl"
              :layers="[layerTransactionZones]"
            />
          </vl-ol-map-tile-layer>

          <vl-ol-map-tile-layer :opacity="0.4">
            <vl-ol-map-tile-wms-source
              :url="roadRegistryWmsUrl"
              :layers="[layerOverlappingTransactionZones]"
            />
          </vl-ol-map-tile-layer>
        </vl-ol-map>
      </vl-column>
    </vl-grid>
  </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import Map from "ol/Map";
import {
  WMS_URL,
  WMS_LAYER_OVERLAPPINGTRANSACTIONZONES,
  WMS_LAYER_TRANSACTIONZONES,
} from "@/environment";

export default defineComponent({
  computed: {
    olMap(): Map {
      return (this.$refs.map as any).olMap as Map;
    },
    roadRegistryWmsUrl(): string {
      return WMS_URL;
    },
    layerOverlappingTransactionZones(): string {
      return WMS_LAYER_OVERLAPPINGTRANSACTIONZONES;
    },
    layerTransactionZones(): string {
      return WMS_LAYER_TRANSACTIONZONES;
    },
  },
});
</script>

<style lang="scss"></style>
