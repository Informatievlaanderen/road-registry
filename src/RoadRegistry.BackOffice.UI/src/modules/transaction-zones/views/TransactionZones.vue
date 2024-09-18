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

          <vl-map-vector-layer>
            <vl-map-vector-source :url="transactionZonesGeoJsonUrl" />
            <vl-map-icon-style color="rgba(183, 171, 31, 1)" color-stroke="rgba(183, 171, 31, 1)" />
            <vl-map-layer-style data-vl-text-feature-attribute-name="description" />
          </vl-map-vector-layer>
          <!-- <vl-map-select-interaction @select="onSelectTransactionZone">
                <vl-map-icon-style mod-highlight />
              </vl-map-select-interaction> -->

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
import Map from "ol/Map";
import { PublicApi } from "@/services";

export default Vue.extend({
  computed: {
    olMap(): Map {
      return (this.$refs.map as any).olMap as Map;
    },
    transactionZonesGeoJsonUrl() {
      return PublicApi.Extracts.getTransactionZonesGeoJsonUrl();
    },
    overlappingTransactionZonesGeoJsonUrl() {
      return PublicApi.Extracts.getOverlappingTransactionZonesGeoJsonUrl();
    },
  },
  methods: {
    onSelectTransactionZone() {
      //console.log("onSelectTransactionZone", arguments);
    },
  }
});
</script>

<style lang="scss"></style>
