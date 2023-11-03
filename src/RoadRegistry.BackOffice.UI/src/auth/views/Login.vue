<template>
  <div>
    <wr-h2>Aanmelden</wr-h2>

    <div class="vl-form-grid vl-form-grid--is-stacked">
      <div class="vl-form-col--12-12">
        <label for="personApiKey">API sleutel</label>
      </div>
      <div class="vl-form-col--12-12">
        <input
          type="text"
          id="personApiKey"
          name="personApiKey"
          class="vl-input-field vl-input-field--block"
          :disabled="isLoginInProgress"
          v-model="apiKey"
        />
      </div>
      <div class="vl-form-col--12-12">
        <vl-action-group>
          <vl-button v-if="isLoginInProgress" mod-loader mod-disabled>Aanmelden</vl-button>
          <vl-button v-else v-on:click="loginApiKey">Aanmelden</vl-button>
          <vl-button v-if="useAcmIdm" @click="loginAcmIdm">Aanmelden met ACM/IDM</vl-button>
        </vl-action-group>
      </div>
      <div class="vl-form-col--12-12">
        <vl-alert v-if="startupError" icon="warning" title="Technische storing" mod-error role="alertdialog">
          <p>We ondervinden technische problemen, gelieve later opnieuw te proberen.</p>
        </vl-alert>
        <vl-alert v-if="loginFailed" icon="warning" title="Inloggen mislukt" mod-error role="alertdialog">
          <p>De ingevoerde API sleutel is ongeldig of heeft geen toegang.</p>
        </vl-alert>
        <vl-alert v-if="acmIdmLoginFailed" icon="warning" title="Inloggen mislukt" mod-error role="alertdialog">
          <p>Aanmelden met ACM/IDM is mislukt of u heeft geen toegang.</p>
        </vl-alert>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { AuthService } from "@/services/auth-service";
import { featureToggles } from "@/environment";

export default Vue.extend({
  props: {
    error: String,
  },
  data() {
    return {
      apiKey: "",
      isLoginInProgress: false,
      loginFailed: false,
      acmIdmLoginFailed: false,
      startupError: false,
    };
  },
  computed: {
    useAcmIdm() {
      return featureToggles.useAcmIdm;
    },
  },
  methods: {
    async loginApiKey() {
      this.isLoginInProgress = true;
      try {
        let isLoggedIn = await AuthService.loginApiKey(this.apiKey, this.$route.query.redirect?.toString());
        if (!isLoggedIn) {
          this.loginFailed = true;
        }
      } finally {
        this.isLoginInProgress = false;
      }
    },
    async loginAcmIdm() {
      try {
        await AuthService.loginAcmIdm(this.$route.query.redirect?.toString());
      } catch (err) {
        this.acmIdmLoginFailed = true;
      }
    },
  },
  mounted() {
    switch (this.error) {
      case "startup_error":
        this.startupError = true;
        break;
      case "acmidm_login_failed":
        this.acmIdmLoginFailed = true;
        break;
    }
  },
});
</script>

<style lang="scss"></style>
