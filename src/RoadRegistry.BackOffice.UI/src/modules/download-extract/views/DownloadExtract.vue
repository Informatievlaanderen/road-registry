<template>
  <div>
    <wr-h2>Wizard extract downloaden</wr-h2>
    <div class="vl-typography">
      <p>Volg de stappen hieronder om een extract van het Wegenregister te downloaden.</p>

      <div v-if="currentStep == steps.Step1">
        <h3>Stap 1: gemeentecontour of eigen contour?</h3>
        <p>
          Wenst u een extract ter grootte van een gemeente, of een extract ter grootte van een willekeurige contour in
          WKT-formaat?
        </p>
        <vl-action-group>
          <vl-button v-on:click="currentStep = steps.Step2_Municipality">Gemeentecontour</vl-button>
          <vl-button v-on:click="currentStep = steps.Step2_Contour">Eigen contour</vl-button>
        </vl-action-group>
      </div>

      <div v-else-if="currentStep == steps.Step2_Municipality">
        <h3>Stap 2: Details van de contour</h3>
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
          <div class="vl-form-col--12-12">
            <p>Wenst u een bufferzone van 100m toe te voegen aan de contour?</p>
          </div>
          <div class="vl-form-col--12-12">
            <label class="vl-checkbox" for="municipality-buffer">
              <input
                class="vl-checkbox__toggle"
                type="checkbox"
                id="municipality-buffer"
                v-model="municipalityFlow.buffer"
              />
              <span class="vl-checkbox__label">
                <i class="vl-checkbox__box" aria-hidden="true"></i>
                Voeg buffer toe
              </span>
            </label>
          </div>
          <vl-action-group>
            <vl-button v-on:click="currentStep = steps.Step1">Vorige</vl-button>
            <vl-button v-if="municipalityFlow.nisCode == ''" mod-disabled>Volgende</vl-button>
            <vl-button v-else v-on:click="currentStep = steps.Step3_Municipality">Volgende</vl-button>
          </vl-action-group>
        </div>
      </div>

      <div v-else-if="currentStep == steps.Step3_Municipality">
        <h3>Stap 3: Beschrijving van het extract</h3>
        <div class="vl-form-grid vl-form-grid--is-stacked">
          <div class="vl-form-col--12-12">
            <p>Wenst u een oplading uit te voeren voor deze extractaanvraag?</p>
          </div>
          <div class="vl-form-col--12-12">
            <label>
              <input v-model="municipalityFlow.isInformative" type="radio" :value="false" />
              Ja, ik wens een oplading uit te voeren
            </label>
          </div>
          <div class="vl-form-col--12-12">
            <label>
              <input v-model="municipalityFlow.isInformative" type="radio" :value="true" />
              Nee, ik vraag een informatief extract aan
            </label>
          </div>
          <div class="vl-form-col--12-12">
            <label for="municipality-description" class="vl-form__label __field__label">
              Geef een beschrijving op van het extract.
            </label>
          </div>
          <div class="vl-form-col--12-12">
            <vl-textarea
              id="municipality-description"
              cols="200"
              rows="4"
              v-model="municipalityFlow.description"
              mod-block
            ></vl-textarea>
          </div>
          <div class="vl-form-col--12-12">
            <vl-action-group>
              <vl-button @click="currentStep = steps.Step2_Municipality">Vorige</vl-button>
              <vl-button
                @click="submitMunicipalityRequest"
                :mod-disabled="
                  isSubmitting || !isDescriptionValid(municipalityFlow.description) || !municipalityFlowHasIsInformative
                "
              >
                Extract aanvragen
              </vl-button>
            </vl-action-group>
          </div>
          <div class="vl-form-col--12-12">
            <vl-alert
              v-if="!isDescriptionValid(municipalityFlow.description)"
              mod-warning
              title="Validatie fouten"
              mod-small
            >
              Gelieve een beschrijving mee te geven van minimaal 5 en maximaal 250 karakters.
            </vl-alert>
            <vl-alert v-if="municipalityFlow.hasGenericError" mod-error mod-small>
              <p>Er is een onverwachte fout opgetreden.</p>
            </vl-alert>
          </div>
        </div>
      </div>

      <div v-show="currentStep == steps.Step2_Contour">
        <h3>Stap 2: Details van de contour</h3>
        <div class="vl-form-grid vl-form-grid--is-stacked">
          <div class="vl-form-col--12-12">
            <p>Geef een contour op waarvoor u het extract wenst op te halen. Dit kan op één van twee manieren:</p>
          </div>
          <div class="vl-form-col--12-12">
            <label>
              <input v-model="contourFlow.contourType" type="radio" value="shp" />
              Laad een contour op in shapefile formaat
            </label>
          </div>
          <div class="vl-form-col--12-12">
            <label>
              <input v-model="contourFlow.contourType" type="radio" value="wkt" />
              Geef een contour op in WKT formaat
            </label>
          </div>
          <template v-if="contourFlow.contourType === 'shp'">
            <div class="vl-form-col--12-12">
              <p>
                <br />
                Laad een shapefile op voor een (multi)polygoon in coördinatensysteem Lambert 1972.
              </p>
              <vl-upload
                ref="vlUpload"
                id="upload-component"
                name="upload-component"
                url="#"
                upload-drag-text="Selecteer de op te laden bestanden (.shp, .prj)"
                upload-label="Shapefile"
                :auto-process="false"
                :options="uploadOptions"
                :max-files-msg="`Je mag maximaal ${uploadOptions.maxFiles} bestanden opladen.`"
                :mod-disabled="isUploading"
                @upload-success="isUploading = false"
                @upload-complete="isUploading = false"
                @upload-canceled="isUploading = false"
                @upload-file-added="uploadFileAdded"
                @upload-file-added-manually="uploadFileAdded"
                @upload-removed-file="uploadFileRemoved"
              />
            </div>
          </template>
          <template v-if="contourFlow.contourType === 'wkt'">
            <div class="vl-form-col--12-12">
              <p>
                <br />
                Geef een contour op (in WKT formaat, coördinatensysteem Lambert 1972) waarvoor u het extract wenst op te
                halen.
              </p>
              <textarea
                class="vl-textarea"
                id="contour-wkt"
                cols="200"
                rows="4"
                v-model="contourFlow.wkt"
                @input="contourFlowWktChanged"
                @paste="contourFlowWktChanged"
              ></textarea>
            </div>
          </template>

          <div class="vl-form-col--12-12">
            <vl-action-group>
              <vl-button @click="currentStep = steps.Step1">Vorige</vl-button>
              <vl-button
                @click="approveStep2()"
                :class="{ 'vl-button--disabled': !contourFlowHasValidInput }"
                :disabled="!contourFlowHasValidInput"
              >
                Volgende
              </vl-button>
            </vl-action-group>
          </div>

          <vl-column v-if="alertInfo.title">
            <vl-alert
              :title="alertInfo.title"
              :mod-success="alertInfo.success"
              :mod-warning="alertInfo.warning"
              :mod-error="alertInfo.error"
            >
              <p>{{ alertInfo.text }}</p>
            </vl-alert>
          </vl-column>
        </div>
      </div>

      <div v-show="currentStep == steps.Step3_Contour">
        <h3>Stap 3: Beschrijving van het extract</h3>
        <div class="vl-form-grid vl-form-grid--is-stacked">
          <div class="vl-form-col--12-12">
            <p>Wenst u een oplading uit te voeren voor deze extractaanvraag?</p>
          </div>
          <div class="vl-form-col--12-12">
            <label>
              <input v-model="contourFlow.isInformative" type="radio" :value="false" />
              Ja, ik wens een oplading uit te voeren
            </label>
          </div>
          <div class="vl-form-col--12-12">
            <label>
              <input v-model="contourFlow.isInformative" type="radio" :value="true" />
              Nee, ik vraag een informatief extract aan
            </label>
          </div>
          <div class="vl-form-col--12-12">
            <label for="contour-description" class="vl-form__label __field__label">
              Geef een beschrijving op van het extract.
            </label>
          </div>
          <div class="vl-form-col--12-12">
            <textarea
              class="vl-textarea"
              id="contour-description"
              cols="200"
              rows="4"
              v-model="contourFlow.description"
              mod-block
            ></textarea>
          </div>
          <div class="vl-form-col--12-12">
            <vl-action-group>
              <vl-button @click="currentStep = steps.Step2_Contour">Vorige</vl-button>
              <vl-button
                @click="submitContourRequest"
                :mod-disabled="
                  isSubmitting || !isDescriptionValid(contourFlow.description) || !contourFlowHasIsInformative
                "
              >
                Extract aanvragen
              </vl-button>
            </vl-action-group>
          </div>

          <div class="vl-form-col--8-12"></div>
          <div class="vl-form-col--6-12">
            <vl-alert
              v-if="!isDescriptionValid(contourFlow.description)"
              mod-warning
              title="Validatie fouten"
              mod-small
            >
              Gelieve een beschrijving mee te geven van minimaal 5 en maximaal 250 karakters.
            </vl-alert>
            <vl-alert v-if="contourFlow.hasGenericError" mod-error mod-small>
              <p>Er is een onverwachte fout opgetreden.</p>
            </vl-alert>
            <vl-alert v-if="contourFlow.hasValidationErrors" mod-error title="Validatie fouten" mod-small>
              <ul>
                <li
                  v-for="contourValidationError in contourFlow.validationErrors.contour"
                  :key="contourValidationError.code"
                >
                  {{ contourValidationError.reason }}
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
import { BackOfficeApi, PublicApi } from "../../../services";
import ValidationUtils from "@/core/utils/validation-utils";
import Municipalities from "../../../types/municipalities";
import RoadRegistry from "../../../types/road-registry";
import RoadRegistryExceptions from "../../../types/road-registry-exceptions";

import Vue from "vue";
import { debounce } from "lodash";

enum WizardSteps {
  Step1 = 1,
  Step2_Municipality,
  Step2_Contour,
  Step3_Municipality,
  Step3_Contour,
}

export default Vue.extend({
  data() {
    const contourTypes = ["shp", "wkt"];

    return {
      steps: WizardSteps,
      currentStep: WizardSteps.Step1,
      municipalities: [] as Municipalities.Gemeenten[],
      municipalityFlow: {
        nisCode: "",
        buffer: false,
        description: "",
        hasGenericError: false,
        isInformative: null as Boolean | null,
      },
      contourFlow: {
        contourTypes,
        contourType: contourTypes[0],
        wkt: "",
        wktIsValid: false,
        wktIsLargerThanMaximumArea: false,
        area: 0,
        areaMaximumSquareKilometers: 0,
        files: [] as Array<File>,
        description: "",
        hasValidationErrors: false,
        validationErrors: {} as RoadRegistry.PerContourValidationErrors,
        hasGenericError: false,
        isInformative: null as Boolean | null,
      },
      validation: {
        description: {
          minLength: 5,
          maxLength: 250,
        },
      },
      isCheckingWkt: false as Boolean,
      isUploading: false as Boolean,
      isSubmitting: false as Boolean,
      debouncedCheckIfContourWktIsValid: () => {},
    };
  },
  computed: {
    uploadOptions() {
      return {
        uploadMultiple: true,
        autoQueue: false,
        autoProcessQueue: false,
        maxFiles: 2,
        maxFilesize: 28672000, //28MB,
        acceptedFiles: ".shp, .prj",
        paramName: "archive",
        headers: {},
      };
    },
    hasAllRequiredUploadFiles(): Boolean {
      const requiredFileExtensions = [".shp", ".prj"];

      let fileNameWithoutExt = "";

      const files = this.contourFlow.files as Array<File>;

      if (files.length !== requiredFileExtensions.length) {
        return false;
      }

      return (
        requiredFileExtensions.filter((ext) => {
          let file = files.find((file) => file.name.toLowerCase().endsWith(ext));
          if (!file) {
            return false;
          }

          let name = file.name.substring(0, file.name.length - ext.length);
          if (!fileNameWithoutExt) {
            fileNameWithoutExt = name;
            return true;
          }

          return name === fileNameWithoutExt;
        }).length === requiredFileExtensions.length
      );
    },
    municipalityFlowHasIsInformative(): Boolean {
      return this.municipalityFlow.isInformative !== null;
    },
    contourFlowHasIsInformative(): Boolean {
      return this.contourFlow.isInformative !== null;
    },
    contourFlowHasValidInput(): Boolean {
      switch (this.contourFlow.contourType) {
        case "wkt":
          return !!this.contourFlow.wkt && this.contourFlow.wktIsValid && !this.contourFlow.wktIsLargerThanMaximumArea;
        case "shp":
          return this.hasAllRequiredUploadFiles;
      }

      throw new Error(`Not implemented contour type: ${this.contourFlow.contourType}`);
    },
    alertInfo(): { success: boolean; warning: boolean; error: boolean; title: string; text: string } {
      const status = {
        success: false,
        warning: false,
        error: false,
        title: "",
        text: "",
      };

      if (this.contourFlow.contourType === "wkt" && this.contourFlow.wkt && !this.isCheckingWkt) {
        if (!this.contourFlow.wktIsValid) {
          status.title = "Ongeldige contour";
          status.text =
            "Gelieve als contour een multipolygoon in WKT-formaat mee te geven die de OGC standaard respecteert.";
          status.error = true;
        } else if (this.contourFlow.wktIsLargerThanMaximumArea) {
          status.title = "Ongeldige contour";
          status.text =
            "Gelieve als contour een maximum van " +
            this.contourFlow.areaMaximumSquareKilometers +
            " km² aan te houden.";
          status.error = true;
        } else {
          status.title = "";
          status.error = false;
        }
      }

      if (
        this.contourFlow.contourType === "shp" &&
        this.contourFlow.files.length > 0 &&
        !this.contourFlowHasValidInput
      ) {
        status.title = "Ongeldige contour";
        status.text =
          "Gelieve precies één .shp bestand en één .prj bestand op te laden. Beide bestanden moeten dezelfde bestandsnaam hebben.";
        status.error = true;
      }

      return status;
    },
  },
  watch: {
    currentStep() {
      switch (this.currentStep) {
        case this.steps.Step3_Municipality:
          this.municipalityFlow.hasGenericError = false;
          break;
        case this.steps.Step3_Contour:
          this.contourFlow.hasValidationErrors = false;
          this.contourFlow.hasGenericError = false;
          break;
      }
    },
  },
  created() {
    this.debouncedCheckIfContourWktIsValid = debounce(this.checkIfContourWktIsValid, 500, { trailing: true });
  },
  async mounted() {
    // fetch paginated data
    this.municipalities = await PublicApi.Municipalities.getAll();

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
    async approveStep2() {
      await this.checkIfContourWktIsValid();
      if (!this.contourFlowHasValidInput) {
        return;
      }

      this.currentStep = this.steps.Step3_Contour;
    },
    contourTypeChanged(value: string) {
      this.contourFlow.contourType = value;
    },
    async submitMunicipalityRequest() {
      this.isSubmitting = true;
      try {
        this.municipalityFlow.hasGenericError = false;

        if (!this.municipalityFlowHasIsInformative) {
          return;
        }

        const requestData: RoadRegistry.DownloadExtractByNisCodeRequest = {
          buffer: this.municipalityFlow.buffer ? 100 : 0,
          nisCode: this.municipalityFlow.nisCode,
          description: this.municipalityFlow.description,
          isInformative: this.municipalityFlow.isInformative as Boolean,
        };

        const response = await PublicApi.Extracts.postDownloadRequestByNisCode(requestData);

        // wait a little bit to give the projection time to process the request to show in the activity feed
        await new Promise((resolve) => {
          setTimeout(resolve, 2000);
        });

        this.$router.push({
          name: "activiteit",
          query: { filter: this.municipalityFlow.description },
        });
      } catch (error) {
        console.error("Submit municipality failed", error);
        this.municipalityFlow.hasGenericError = true;
      } finally {
        this.isSubmitting = false;
      }
    },

    async submitContourRequest() {
      this.isSubmitting = true;
      try {
        this.contourFlow.hasValidationErrors = false;
        this.contourFlow.hasGenericError = false;

        if (!this.contourFlowHasIsInformative) {
          return;
        }

        let response: RoadRegistry.DownloadExtractResponse;

        switch (this.contourFlow.contourType) {
          case "shp":
            {
              const requestData: RoadRegistry.DownloadExtractByFileRequest = {
                files: this.contourFlow.files,
                description: this.contourFlow.description,
                isInformative: this.contourFlow.isInformative as Boolean,
              };
              response = await BackOfficeApi.Extracts.postDownloadRequestByFile(requestData);
            }
            break;
          case "wkt":
            {
              const requestData: RoadRegistry.DownloadExtractByContourRequest = {
                contour: this.contourFlow.wkt,
                description: this.contourFlow.description,
                isInformative: this.contourFlow.isInformative as Boolean,
              };

              response = await PublicApi.Extracts.postDownloadRequestByContour(requestData);
            }
            break;
          default:
            throw new Error(`Not implemented contour type: ${this.contourFlow.contourType}`);
        }

        this.$router.push({ name: "activiteit", query: { filter: this.contourFlow.description } });
      } catch (exception) {
        if (exception instanceof RoadRegistryExceptions.RequestExtractPerContourError) {
          this.contourFlow.hasValidationErrors = true;
          this.contourFlow.validationErrors = {
            contour: ValidationUtils.convertValidationErrorsToContourValidationErrors(exception.error.validationErrors),
          };
        } else {
          console.error("Submit contour failed", exception);
          this.contourFlow.hasGenericError = true;
        }
      } finally {
        this.isSubmitting = false;
      }
    },

    isDescriptionValid(description: string): boolean {
      const validationRules = this.validation.description;

      if (!description) return false;
      if (description.length < validationRules.minLength) return false;
      if (description.length > validationRules.maxLength) return false;

      return true;
    },

    uploadFileAdded(file: File) {
      this.contourFlow.files.push(file);
    },
    uploadFileRemoved(file: File) {
      let index = this.contourFlow.files.indexOf(file);
      this.contourFlow.files.splice(index, 1);
    },
    async contourFlowWktChanged() {
      this.resetContourFlow();
      this.isCheckingWkt = true;
      this.debouncedCheckIfContourWktIsValid();
    },
    resetContourFlow() {
      this.contourFlow.wktIsValid = false;
      this.contourFlow.wktIsLargerThanMaximumArea = false;
      this.contourFlow.area = 0;
      this.contourFlow.areaMaximumSquareKilometers = 0;
    },
    async checkIfContourWktIsValid(): Promise<RoadRegistry.ValidateWktResponse | void> {
      if (!this.contourFlow.wkt) {
        return;
      }

      this.resetContourFlow();
      this.isCheckingWkt = true;

      try {
        let response = await BackOfficeApi.Information.postValidateWkt(this.contourFlow.wkt);
        this.contourFlow.wktIsValid = response.isValid;
        this.contourFlow.wktIsLargerThanMaximumArea = response.isLargerThanMaximumArea;
        this.contourFlow.area = response.area;
        this.contourFlow.areaMaximumSquareKilometers = response.areaMaximumSquareKilometers;
      } catch (err) {
        console.error("WKT is invalid", err);
        this.contourFlow.wktIsValid = false;
        this.contourFlow.wktIsLargerThanMaximumArea = false;
        this.contourFlow.area = 0;
        this.contourFlow.areaMaximumSquareKilometers = 0;
      } finally {
        this.isCheckingWkt = false;
      }
    },
  },
});
</script>

<style lang="scss"></style>
