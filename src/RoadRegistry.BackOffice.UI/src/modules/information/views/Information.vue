<template>
  <div class="vl-u-table-overflow">
    <vl-data-table>
      <thead>
        <tr>
          <th>Gegeven</th>
          <th>Waarde</th>
        </tr>
      </thead>
      <tbody>
        <tr>
          <td># organisaties</td>
          <td>
            <div v-if="isLoading"><vl-loader mod-inline mod-small mod-message-hidden /></div>
            <div v-else>{{ this.information.organizationCount }}</div>
          </td>
        </tr>
        <tr>
          <td># wegknopen</td>
          <td>
            <div v-if="isLoading"><vl-loader mod-inline mod-small mod-message-hidden /></div>
            <div v-else>{{ this.information.roadNodeCount }}</div>
          </td>
        </tr>
        <tr>
          <td># wegsegmenten</td>
          <td>
            <div v-if="isLoading"><vl-loader mod-inline mod-small mod-message-hidden /></div>
            <div v-else>{{ this.information.roadSegmentCount }}</div>
          </td>
        </tr>
        <tr>
          <td># ongelijkgrondse kruisingen</td>
          <td>
            <div v-if="isLoading"><vl-loader mod-inline mod-small mod-message-hidden /></div>
            <div v-else>{{ this.information.gradeSeparatedJunctionCount }}</div>
          </td>
        </tr>
      </tbody>
    </vl-data-table>
  </div>
</template>


<script lang="ts">
import Vue from "vue";
import { PublicApi } from "../../../services";
import RoadRegistry from "../../../types/road-registry";

export default Vue.extend({
  data() {
    return {
      isLoading: true,
      information: {} as RoadRegistry.RoadNetworkInformationResponse,
    };
  },
  async mounted() {
    this.information = await PublicApi.Information.getInformation();
    this.isLoading = false;
  },
});
</script>

<style lang="scss">
</style>
