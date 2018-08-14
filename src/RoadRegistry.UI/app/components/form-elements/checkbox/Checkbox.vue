<template>
  <label :class="{
        checkbox: true,
        'checkbox--block': this.modIsBlock,
        'checkbox--error': this.modIsError,
        'checkbox--disabled': this.disabled,
      }">
    <input
      type="checkbox"
      :name="name"
      :id="id"
      class="checkbox__toggle"
      :disabled="disabled"
      :checked="checked"
      :value="value">
      <span></span>
    <wr-label class="label" :for="id">{{ label }}</wr-label>
  </label>
</template>

<script>

export default {
  components: {
  },
  methods: {
  },
  name: 'checkbox',
  props: {
    name: {
      default: '',
      type: String,
    },
    value: {
      default: '',
      type: String,
    },
    id: {
      default: '',
      type: String,
    },
    disabled: {
      default: false,
      type: Boolean,
    },
    label: {
      default: '',
      type: String,
    },
    checked: {
      default: false,
      type: Boolean,
    },
    modIsBlock: {
      default: false,
      type: Boolean,
    },
    modIsError: {
      default: false,
      type: Boolean,
    },
  },
  data() {
    return {
    };
  },
};
</script>
