<template>
  <div class="vl-grid vl-grid--align-center grid-summary">
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ summary.roadNodes.added }}
          </div>
          <div class="vl-infotext__text">Toegevoegde wegknopen</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ summary.roadNodes.modified }}
          </div>
          <div class="vl-infotext__text">Gewijzigde wegknopen</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ summary.roadNodes.removed }}
          </div>
          <div class="vl-infotext__text">Verwijderde wegknopen</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ summary.roadSegments.added }}
          </div>
          <div class="vl-infotext__text">Toegevoegde wegsegmenten</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ summary.roadSegments.modified }}
          </div>
          <div class="vl-infotext__text">Gewijzigde wegsegmenten</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ summary.roadSegments.removed }}
          </div>
          <div class="vl-infotext__text">Verwijderde wegsegmenten</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ summary.gradeSeparatedJunctions.added }}
          </div>
          <div class="vl-infotext__text">Toegevoegde ongelijkgrondse kruisingen</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ summary.gradeSeparatedJunctions.modified }}
          </div>
          <div class="vl-infotext__text">Gewijzigde ongelijkgrondse kruisingen</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ summary.gradeSeparatedJunctions.removed }}
          </div>
          <div class="vl-infotext__text">Verwijderde ongelijkgrondse kruisingen</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({
  name: "ActivitySummary",
  props: {
    summary: {} as any,
  },
  data() {
    return {};
  }
});
</script>
