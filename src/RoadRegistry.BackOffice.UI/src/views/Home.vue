<template>
  <div>
    <div class="vl-alert" role="alertdialog">
      <div class="vl-alert__icon">
        <i class="vl-vi vl-vi-warning" aria-hidden="true"></i>
      </div>
      <div class="vl-alert__content">
        <p class="vl-alert__title">Opgelet!</p>
        <div class="vl-alert__message">
          <p>U heeft geen rechten om deze actie uit te voeren. <a href="#">Vraag rechten aan</a>.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";

export default defineComponent({
  data() {
    return {};
  },
  async mounted() {},
});
</script>

<style lang="scss"></style>
