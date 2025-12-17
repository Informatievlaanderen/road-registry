<template>
  <div>
    <vl-alert v-if="errors.length" icon="" role="alertdialog" mod-error mod-small>
      <p v-for="problem in errors" :key="problem.text">{{ problem.text }}</p>
    </vl-alert>
    <vl-alert v-if="warnings.length" icon="" role="alertdialog" mod-small>
      <p v-for="problem in warnings" :key="problem.text">{{ problem.text }}</p>
    </vl-alert>
  </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";

export default defineComponent({
  name: "ActivityProblems",
  props: {
    problems: Array as () => Array<any>,
  },
  data() {
    return {};
  },
  computed: {
    errors() {
      return (this.problems || []).filter((x) => x.severity == "Error");
    },
    warnings() {
      return (this.problems || []).filter((x) => x.severity == "Warning");
    },
  },
});
</script>
