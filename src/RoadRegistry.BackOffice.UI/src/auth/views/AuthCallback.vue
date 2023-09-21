<template>
  <div>
    <wr-h2>Even geduld aub...</wr-h2>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { AuthService } from "@/services/auth-service";

export default Vue.extend({
  props: {
    code: String,
  },
  async mounted() {
    try {
      await AuthService.completeAcmIdmLogin(this.code);
    } catch (err) {
      console.error("Completing login failed", err);
      this.$router.push({name: 'login', query: {
        error: 'acmidm_login_failed'
      }});
    }
  },
});
</script>

<style lang="scss"></style>
