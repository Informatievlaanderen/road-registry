<template>
  <div :class="classes">
    <input
      class="input-field"
      type="text"
      :placeholder="placeholder"
      :name="name"
      data-datepicker
      :data-datepicker-min="min"
      :data-datepicker-max="max"
      v-validate="validation" />
  </div>
</template>

<script>
export default {
  inject: ['$validator'],
  name: 'date-picker',
  props: {
    name: {
      default: '',
      type: String,
    },
    placeholder: {
      default: '',
      type: String,
    },
    min: {
      default: '',
      type: String,
    },
    max: {
      default: '',
      type: String,
    },
    validation: {
      default: '',
      type: String,
    },
  },
  data() {
    return {
      classes: {
        datepicker: true,
      },
    };
  },
  mounted() {
    // eslint-disable-next-line
    vl.datepicker.dressAll();
  },
};
</script>
