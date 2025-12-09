<template>
  <div>
    <wr-h2>Afmelden</wr-h2>

    <div class="vl-form-grid vl-form-grid--is-stacked">
      <div class="vl-form-col--12-12">
        <vl-alert icon="warning" title="Opgelet!" mod-warning role="alertdialog">
          <p>U bent afgemeld van onze diensten.</p>
        </vl-alert>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import { AuthService } from "@/services/auth-service";

export default defineComponent({
  data() {
    return {
      isLogoutInProgress: false,
    };
  },
  props: {
    redirect: String,
  },
  methods: {
    async logout() {
      this.isLogoutInProgress = true;
      try {
        await AuthService.logout();
      } finally {
        this.isLogoutInProgress = false;
      }
    },
  },
});
</script>
