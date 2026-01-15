<template>
  <ExtractDetails v-if="render" @reload="reload()" />
</template>

<script lang="ts">
import { defineComponent } from "vue";
import ExtractDetails from "./ExtractDetails.vue";

export default defineComponent({
  components: {
    ExtractDetails,
  },
  data() {
    return {
      render: true,
    };
  },
  methods: {
    async reload() {
      this.render = false;
      await this.$nextTick();
      await new Promise(resolve => setTimeout(resolve, 250));
      this.render = true;
    },
  },
});
</script>
