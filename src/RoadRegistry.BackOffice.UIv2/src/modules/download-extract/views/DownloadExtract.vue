<template>
  <div>
    <div class="vl-typography">
      <h2>Wizard extract downloaden</h2>
      <p>Volg de stappen hieronder om een extract van het Wegenregister te downloaden.</p>

      <div v-if="currentStep == steps.Step1">
        <h3>Stap 1: gemeentecontour of eigen contour?</h3>
        <p>
          Wenst u een extract ter grootte van een gemeente, of een extract ter grootte van een willekeurige contour in
          WKT-formaat?
        </p>
        <div class="vl-form-grid">
          <div class="vl-form-col--2-12">
            <button class="vl-button vl-button--block" vl-button v-on:click="currentStep = steps.Step2_Municipality">
              <span class="vl-button__label">Gemeentecontour</span>
            </button>
          </div>
          <div class="vl-form-col--2-12">
            <button class="vl-button vl-button--block" vl-button v-on:click="currentStep = steps.Step2_Contour">
              <span class="vl-button__label">Eigen contour</span>
            </button>
          </div>
        </div>
      </div>

      <div v-else-if="currentStep == steps.Step2_Municipality">
        <h3>Stap 2: Details van de contour</h3>
        <div class="vl-form-grid vl-form-grid--is-stacked">
          <div class="vl-form-col--12-12">
            <label for="select-municipality" class="vl-form__label __field__label">Vul hieronder de gemeente in waarvoor u de contour wenst op te halen</label>
          </div>
          <div class="vl-form-col--3-12">
            <select id="select-municipality" class="vl-select--block vl-select vl-select--block" v-model="municipalityFlow.nisCode">
              <option v-for="municipality in municipalities" :key="municipality.identificator.objectId" :value="municipality.identificator.objectId">
                {{ municipality.gemeentenaam.geografischeNaam.spelling }}
              </option>
            </select>
          </div>
          <div class="vl-form-col--9-12"></div>
          <div class="vl-form-col--12-12">
            <p>Wenst u een bufferzone van 100m toe te voegen aan de contour?</p>
          </div>
          <div class="vl-form-col--12-12">
            <label class="vl-checkbox" for="municipality-buffer">
              <input class="vl-checkbox__toggle" type="checkbox" id="municipality-buffer" v-model="municipalityFlow.buffer" />
              <span class="vl-checkbox__label">
                <i class="vl-checkbox__box" aria-hidden="true"></i>
                Voeg buffer toe
              </span>
            </label>
          </div>
          <div class="vl-form-col--2-12">
            <button class="vl-button vl-button--block" vl-button v-on:click="currentStep = steps.Step1">
              <span class="vl-button__label">Vorige</span>
            </button>
          </div>
          <div class="vl-form-col--2-12">
            <button class="vl-button vl-button--block" vl-button v-on:click="currentStep = steps.Step3_Municipality" v-bind:class="{ 'vl-button--disabled': municipalityFlow.nisCode == '' }" :disabled="municipalityFlow.nisCode == ''">
              <span class="vl-button__label">Volgende</span>
            </button>
          </div>
        </div>
      </div>

      <div v-else-if="currentStep == steps.Step3_Municipality">
        <h3>Stap 3: Beschrijving van het extract</h3>
        <div class="vl-form-grid vl-form-grid--is-stacked">
          <div class="vl-form-col--12-12">
            <label for="municipality-description" class="vl-form__label __field__label">Geef een beschrijving op van het extract.</label>
          </div>
          <div class="vl-form-col--3-12">
            <textarea class="vl-textarea" id="municipality-description" value="" cols="40" rows="4" v-model="municipalityFlow.description"></textarea>
          </div>
          <div class="vl-form-col--9-12"></div>
          <div class="vl-form-col--2-12">
            <button class="vl-button vl-button--block" vl-button v-on:click="currentStep = steps.Step2_Municipality">
              <span class="vl-button__label">Vorige</span>
            </button>
          </div>
          <div class="vl-form-col--2-12">
            <button class="vl-button vl-button--block" vl-button v-on:click="submitMunicipalityRequest">
              <span class="vl-button__label">Extract aanvragen</span>
            </button>
          </div>
        </div>
      </div>

      <div v-else-if="currentStep == steps.Step2_Contour">
        <h3>Stap 2: Details van de contour</h3>
        <div class="vl-form-grid vl-form-grid--is-stacked">
          <div class="vl-form-col--12-12">
            <label for="contour-wkt" class="vl-form__label __field__label">Geef een contour op (in WKT-formaat, coördinatensysteem Lambert 1972) waarvoor u het extract wenst op te halen:</label>
          </div>
          <div class="vl-form-col--3-12">
            <textarea class="vl-textarea" id="contour-wkt" value="" cols="40" rows="4" v-model="contourFlow.wkt"></textarea>
          </div>
          <div class="vl-form-col--9-12"></div>
          <div class="vl-form-col--12-12">
            <p>Wenst u een bufferzone van 100m toe te voegen aan de contour?</p>
          </div>
          <div class="vl-form-col--12-12">
            <label class="vl-checkbox" for="contour-buffer">
              <input class="vl-checkbox__toggle" type="checkbox" id="contour-buffer" v-model="contourFlow.buffer" />
              <span class="vl-checkbox__label">
                <i class="vl-checkbox__box" aria-hidden="true"></i>
                Voeg buffer toe
              </span>
            </label>
          </div>
          <div class="vl-form-col--2-12">
            <button class="vl-button vl-button--block" vl-button v-on:click="currentStep = steps.Step1">
              <span class="vl-button__label">Vorige</span>
            </button>
          </div>
          <div class="vl-form-col--2-12">
            <button class="vl-button vl-button--block" vl-button v-on:click="currentStep = steps.Step3_Contour" v-bind:class="{ 'vl-button--disabled': contourFlow.wkt == '' }" :disabled="contourFlow.wkt == ''">
              <span class="vl-button__label">Volgende</span>
            </button>
          </div>
        </div>
      </div>

      <div v-else-if="currentStep == steps.Step3_Contour">
        <h3>Stap 3: Beschrijving van het extract</h3>
        <div class="vl-form-grid vl-form-grid--is-stacked">
          <div class="vl-form-col--12-12">
            <label for="municipality-description" class="vl-form__label __field__label">Geef een beschrijving op van het extract.</label>
          </div>
          <div class="vl-form-col--3-12">
            <textarea class="vl-textarea" id="municipality-description" value="" cols="40" rows="4" v-model="contourFlow.description"></textarea>
          </div>
          <div class="vl-form-col--9-12"></div>
          <div class="vl-form-col--2-12">
            <button class="vl-button vl-button--block" vl-button v-on:click="currentStep = steps.Step2_Contour">
              <span class="vl-button__label">Vorige</span>
            </button>
          </div>
          <div class="vl-form-col--2-12">
            <button class="vl-button vl-button--block" vl-button v-on:click="submitContourRequest">
              <span class="vl-button__label">Extract aanvragen</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { PublicApi } from "../../../services";
import Municipalities from "../../../types/municipalities";
import RoadRegistry from "../../../types/road-registry";

import Vue from "vue";

enum WizardSteps {
  Step1 = 1,
  Step2_Municipality,
  Step2_Contour,
  Step3_Municipality,
  Step3_Contour,
}

export default Vue.extend({
  data() {
    return {
      steps: WizardSteps,
      currentStep: WizardSteps.Step1,
      municipalities: [] as Municipalities.Gemeenten[],
      municipalityFlow: {
        nisCode: '',
        buffer: false,
        description: ''
      },
      contourFlow: {
        wkt: '',
        buffer: false
      }
    };
  },
  async mounted() {
    // fetch paginated data
    let municipalitiesResponse = await PublicApi.Municipalities.getList();
    this.municipalities = this.municipalities.concat(municipalitiesResponse.gemeenten);

    while (municipalitiesResponse.volgende != null) {
      municipalitiesResponse = await PublicApi.Municipalities.getList(municipalitiesResponse.volgende);
      this.municipalities = this.municipalities.concat(municipalitiesResponse.gemeenten);
    }

    // sort
    this.municipalities = this.municipalities.sort((m1, m2) => {
      if (m1.gemeentenaam.geografischeNaam.spelling > m2.gemeentenaam.geografischeNaam.spelling) {
        return 1;
      }
      if (m1.gemeentenaam.geografischeNaam.spelling < m2.gemeentenaam.geografischeNaam.spelling) {
        return -1;
      }
      return 0;
    });
  },
  methods: {
    async submitMunicipalityRequest() {
      const requestData: RoadRegistry.DownloadExtractByNisCodeRequest = {
        buffer: this.municipalityFlow.buffer ? 100 : 0,
        nisCode: this.municipalityFlow.nisCode
      };

      const response = await PublicApi.Extracts.postDownloadRequestByNisCode(requestData);
      this.$router.push({name: 'activiteit', params: { downloadId: response.downloadId }});
    },
    async submitContourRequest() {
      const requestData: RoadRegistry.DownloadExtractByContourRequest = {
        buffer: this.contourFlow.buffer ? 100 : 0,
        contour: this.contourFlow.wkt
      };

      const response = await PublicApi.Extracts.postDownloadRequestByContour(requestData);
      this.$router.push({name: 'activiteit', params: { downloadId: response.downloadId }});
    }
  }
});
</script>

<style lang="scss">
</style>
