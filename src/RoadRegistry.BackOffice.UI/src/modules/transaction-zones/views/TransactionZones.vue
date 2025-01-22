<template>
  <div>
    <wr-h2>Bijwerkingszones</wr-h2>
    <wr-h3>Bijwerkingskaart</wr-h3>

    <vl-grid mod-stacked>
      <vl-column>
        <vl-ol-map ref="map" mod-boxed map-zoomable map-expandable>
          <vl-map-tile-layer>
            <vl-map-tile-wms-source url="https://geo.api.vlaanderen.be/GRB-basiskaart/wms" />
          </vl-map-tile-layer>

          <vl-map-tile-layer>
            <vl-map-tile-wms-source :url="transactionZonesWmsUrl" />
          </vl-map-tile-layer>
        </vl-ol-map>
      </vl-column>
    </vl-grid>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import Map from "ol/Map";
import { WMS_TRANSACTIONZONES } from "@/environment";

export default Vue.extend({
  computed: {
    olMap(): Map {
      return (this.$refs.map as any).olMap as Map;
    },
    transactionZonesWmsUrl(): string {
      return WMS_TRANSACTIONZONES;
    }
  }
});
</script>

<style lang="scss"></style>
