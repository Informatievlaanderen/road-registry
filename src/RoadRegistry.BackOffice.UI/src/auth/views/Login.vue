<template>
    <div>
        <div class="vl-typography">
            <h2>Aanmelden</h2>
        </div>

        <div class="vl-form-grid vl-form-grid--is-stacked">
            <div class="vl-form-col--12-12">
                <label for="personApiKey">API sleutel</label>
            </div>
            <div class="vl-form-col--12-12">
                <input type="text" id="personApiKey" name="personApiKey" class="vl-input-field vl-input-field--block"
                    :disabled="isLoginInProgress"
                    v-model="apiKey" />
            </div>
            <div class="vl-form-col--12-12">
                <vl-action-group>
                    <vl-button v-if="isLoginInProgress" mod-loader mod-disabled>Aanmelden</vl-button>
                    <vl-button v-else v-on:click="login">Aanmelden</vl-button>
                </vl-action-group>
            </div>
            <div class="vl-form-col--12-12">
                <vl-alert icon="warning" title="Opgelet!" mod-warning role="alertdialog">
                    <p>Het wegenregister biedt geen ondersteuning meer voor gebruikersnaam en wachtwoord.</p>
                    <p>Gelieve uzelf te identificeren met behulp van uw persoonlijke API sleutel.</p>
                </vl-alert>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { AuthService } from "../AuthService";

export default Vue.extend({
    data() {
        return {
            apiKey: "",
            isLoginInProgress: false,
        };
    },
    methods: {
        async login() {
            this.isLoginInProgress = true;
            // await AuthService.login(this.apiKey);
            // this.isLoginInProgress = false;
        },
    },
});

</script>

<style lang="scss">
</style>
