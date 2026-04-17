<template>
  <div class="vl-grid vl-grid--align-center grid-summary">
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ roadNodes.added }}
          </div>
          <div class="vl-infotext__text">Toegevoegde wegknopen</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ roadNodes.modified }}
          </div>
          <div class="vl-infotext__text">Gewijzigde wegknopen</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ roadNodes.removed }}
          </div>
          <div class="vl-infotext__text">Verwijderde wegknopen</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ roadSegments.added }}
          </div>
          <div class="vl-infotext__text">Toegevoegde wegsegmenten</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ roadSegments.modified }}
          </div>
          <div class="vl-infotext__text">Gewijzigde wegsegmenten</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ roadSegments.removed }}
          </div>
          <div class="vl-infotext__text">Verwijderde wegsegmenten</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ gradeSeparatedJunctions.added }}
          </div>
          <div class="vl-infotext__text">Toegevoegde ongelijkgrondse kruisingen</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ gradeSeparatedJunctions.modified }}
          </div>
          <div class="vl-infotext__text">Gewijzigde ongelijkgrondse kruisingen</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ gradeSeparatedJunctions.removed }}
          </div>
          <div class="vl-infotext__text">Verwijderde ongelijkgrondse kruisingen</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ gradeJunctions.added }}
          </div>
          <div class="vl-infotext__text">Toegevoegde gelijkgrondse kruisingen</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ gradeJunctions.modified }}
          </div>
          <div class="vl-infotext__text">Gewijzigde gelijkgrondse kruisingen</div>
        </div>
      </div>
    </div>
    <div class="vl-col--4-12 vl-u-align-center">
      <div class="vl-infotext-wrapper">
        <div class="vl-infotext">
          <div class="vl-infotext__value" data-vl-infotext-value>
            {{ gradeJunctions.removed }}
          </div>
          <div class="vl-infotext__text">Verwijderde gelijkgrondse kruisingen</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";

const getCount = (value: any): number => {
  if (Array.isArray(value)) {
    return value.length;
  }

  return value;
};

export default defineComponent({
  name: "ActivitySummary",
  props: {
    summary: {} as any,
  },
  computed: {
    roadNodes() {
      return {
        added: getCount(this.summary.roadNodes.added),
        modified: getCount(this.summary.roadNodes.modified),
        removed: getCount(this.summary.roadNodes.removed),
      };
    },
    roadSegments() {
      return {
        added: getCount(this.summary.roadSegments.added),
        modified: getCount(this.summary.roadSegments.modified),
        removed: getCount(this.summary.roadSegments.removed),
      };
    },
    gradeSeparatedJunctions() {
      return {
        added: getCount(this.summary.gradeSeparatedJunctions.added),
        modified: getCount(this.summary.gradeSeparatedJunctions.modified),
        removed: getCount(this.summary.gradeSeparatedJunctions.removed),
      };
    },
    gradeJunctions() {
      return {
        added: getCount(this.summary.gradeJunctions?.added || []),
        modified: getCount(this.summary.gradeJunctions?.modified || []),
        removed: getCount(this.summary.gradeJunctions?.removed || []),
      };
    },
  },
});
</script>
