<template>
  <div>
    <wr-h2>Wizard extract downloaden</wr-h2>
    <div class="vl-typography">
      <p>Volg de stappen hieronder om een extract van het Wegenregister te downloaden.</p>

      <div v-if="currentStep == steps.Step2_Municipality">
        <h3>Stap 1: Details van de contour</h3>

        <div v-if="!initializeCompleted">
          <vl-region>
            <div v-vl-align:center>
              <vl-loader message="Uw pagina is aan het laden" />
            </div>
          </vl-region>
        </div>
        <div v-else>
          <div class="vl-form-grid vl-form-grid--is-stacked">
            <div class="vl-form-col--12-12">
              <label for="select-municipality" class="vl-form__label __field__label"
                >Vul hieronder de gemeente in waarvoor u de contour wenst op te halen</label
              >
            </div>
            <div class="vl-form-col--3-12">
              <select
                id="select-municipality"
                class="vl-select--block vl-select vl-select--block"
                v-model="municipalityFlow.nisCode"
              >
                <option
                  v-for="municipality in municipalities"
                  :key="municipality.identificator.objectId"
                  :value="municipality.identificator.objectId"
                >
                  {{ municipality.gemeentenaam.geografischeNaam.spelling }}
                </option>
              </select>
            </div>
            <div class="vl-form-col--9-12"></div>
            <vl-action-group>
              <vl-button v-if="municipalityFlow.nisCode == ''" mod-disabled>Volgende</vl-button>
              <vl-button v-else @click="currentStep = steps.Step2_Details">Volgende</vl-button>
            </vl-action-group>
          </div>
        </div>
      </div>

      <div v-else-if="currentStep == steps.Step2_Details">
        <h3>Stap 2: Beschrijving van het extract</h3>
        <div class="vl-form-grid vl-form-grid--is-stacked">
          <div class="vl-form-col--12-12">
            <p>Wenst u een oplading uit te voeren voor deze extractaanvraag?</p>
          </div>
          <div class="vl-form-col--12-12">
            <label>
              <input
                v-model="municipalityFlow.isInformative"
                type="radio"
                :value="false"
              />
              Ja, ik wens een oplading uit te voeren
            </label>
          </div>
          <div class="vl-form-col--12-12">
            <label>
              <input
                v-model="municipalityFlow.isInformative"
                type="radio"
                :value="true"
              />
              Nee, ik vraag een informatief extract aan
            </label>
          </div>
          <div class="vl-form-col--12-12">
            <label for="municipality-referentie" class="vl-form__label __field__label">
              De beschrijving van uw extract begint met de naam van de gekozen gemeente. Voeg indien gewenst een eigen referentie toe aan deze beschrijving (max. 100 karakters):
            </label>
          </div>
          <div class="vl-form-col--12-12">
            <input
              id="municipality-referentie"
              v-model="municipalityFlow.referentie"
              class="vl-input-field vl-input-field--block" />

            <div v-if="!isReferentieValid(municipalityFlow.referentie)">
              <vl-alert mod-warning mod-small>
                Gelieve een optionele referentie mee te geven van maximaal {{validation.referentie.maxLength}} karakters.
              </vl-alert>
            </div>
          <div class="vl-form-col--12-12">
            <label class="vl-form__label __field__label">
              Beschrijving van het extract:
            </label>
          </div>
            <div class="vl-form-col--12-12">{{municipalityFlowDescription}}</div>
          </div>

          <div class="vl-form-col--12-12">
            <vl-action-group>
              <vl-button @click="currentStep = steps.Step2_Municipality">Vorige</vl-button>
              <vl-button
                @click="submitMunicipalityRequest"
                :mod-disabled="
                  isSubmitting ||
                  !isReferentieValid(municipalityFlow.referentie) ||
                  !municipalityFlowHasIsInformative
                "
              >
                Extract aanvragen
              </vl-button>
            </vl-action-group>
          </div>
          <div class="vl-form-col--12-12">
            <vl-alert v-if="municipalityFlow.hasGenericError" mod-error mod-small>
              <p>Er is een onverwachte fout opgetreden.</p>
            </vl-alert>
            <vl-alert v-if="municipalityFlow.validationErrors.length" mod-error title="Validatie fouten" mod-small>
              <ul>
                <li v-for="validationError in municipalityFlow.validationErrors" :key="validationError.code">
                  {{ validationError.reason }}
                </li>
              </ul>
            </vl-alert>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { PublicApi } from "@/services";
import ValidationUtils from "@/core/utils/validation-utils";
import Municipalities from "@/types/municipalities";
import RoadRegistry from "@/types/road-registry";
import RoadRegistryExceptions from "@/types/road-registry-exceptions";
import { WR_ENV } from "@/environment";

import { defineComponent } from "vue";

enum InwinningWizardSteps {
  Step2_Municipality,
  Step2_Details,
}

export default defineComponent({
  data() {
    return {
      initializeCompleted: false,
      steps: InwinningWizardSteps,
      currentStep: InwinningWizardSteps.Step2_Municipality,
      municipalities: [] as Municipalities.Gemeenten[],
      municipalityFlow: {
        nisCode: "",
        referentie: "",
        validationErrors: [] as RoadRegistry.ValidationError[],
        hasGenericError: false,
        isInformative: null as boolean | null,
      },
      validation: {
        referentie: {
          maxLength: 100,
        },
      },
      isCheckingOverlap: false as boolean,
      isCheckingWkt: false as boolean,
      isUploading: false as boolean,
      isSubmitting: false as boolean,
    };
  },
  computed: {
    municipalityFlowHasIsInformative(): boolean {
      return this.municipalityFlow.isInformative !== null;
    },
    municipalityFlowDescription(): string {
      if (!this.municipalityFlow.nisCode) {
        return "";
      }

      let municipality = this.municipalities.find((x) => x.identificator.objectId === this.municipalityFlow.nisCode);
      if (!municipality) {
        return "";
      }

      return `${municipality.gemeentenaam.geografischeNaam.spelling} ${this.municipalityFlow.referentie}`.trim();
    },
  },
  watch: {
    currentStep() {
      switch (this.currentStep) {
        case this.steps.Step2_Details:
          this.municipalityFlow.validationErrors = [];
          this.municipalityFlow.hasGenericError = false;
          break;
      }
    },
  },
  async mounted() {
    await this.loadMunicipalities();
    this.initializeCompleted = true;
  },
  methods: {
    async loadMunicipalities() {
      let municipalities = [] as Municipalities.Gemeenten[];
      try {
        municipalities = await PublicApi.Municipalities.getAll();
      } catch (err) {
        if (WR_ENV === "development") {
          municipalities = [];
        } else {
          throw err;
        }
      }

      if (municipalities.length === 0) {
        municipalities = [
          {
            identificator: { id: "", naamruimte: "", objectId: "11001", versieId: "" },
            detail: "",
            gemeentenaam: {
              geografischeNaam: {
                spelling: "Aartselaar",
                taal: Municipalities.Taal.Nl,
              },
            },
            gemeenteStatus: Municipalities.GemeenteStatus.InGebruik,
          },
        ];
      }

      let availableNisCodes = await PublicApi.Inwinning.getNisCodes();
      municipalities = municipalities.filter((x) => ~availableNisCodes.indexOf(x.identificator.objectId));

      this.municipalities = municipalities.sort((m1, m2) => {
        if (m1.gemeentenaam.geografischeNaam.spelling > m2.gemeentenaam.geografischeNaam.spelling) {
          return 1;
        }
        if (m1.gemeentenaam.geografischeNaam.spelling < m2.gemeentenaam.geografischeNaam.spelling) {
          return -1;
        }
        return 0;
      });
    },
    async submitMunicipalityRequest() {
      this.isSubmitting = true;
      try {
        this.municipalityFlow.validationErrors = [];
        this.municipalityFlow.hasGenericError = false;

        if (!this.municipalityFlowHasIsInformative) {
          return;
        }

        const requestData: RoadRegistry.ExtractDownloadaanvraagPerNisCodeBody = {
          nisCode: this.municipalityFlow.nisCode,
          beschrijving: this.municipalityFlowDescription,
          informatief: this.municipalityFlow.isInformative as boolean,
        };

        let downloadExtractResponse = await PublicApi.Inwinning.requestExtract(requestData);

        this.$router.push({
          name: "inwinningExtractDetails",
          params: { downloadId: downloadExtractResponse.downloadId },
        });
      } catch (exception) {
        if (exception instanceof RoadRegistryExceptions.BadRequestError) {
          this.municipalityFlow.validationErrors = ValidationUtils.convertValidationErrorsToArray(
            exception.error.validationErrors
          );
        } else {
          console.error("Submit municipality failed", exception);
          this.municipalityFlow.hasGenericError = true;
        }
      } finally {
        this.isSubmitting = false;
      }
    },
    isReferentieValid(referentie: string): boolean {
      const validationRules = this.validation.referentie;

      if (!referentie) return true;
      if (referentie.length > validationRules.maxLength) return false;

      return true;
    },
  },
});
</script>
