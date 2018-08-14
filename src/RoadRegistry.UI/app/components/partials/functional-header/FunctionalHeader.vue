<template>
  <header :class="classes">
    <register-layout :mod-is-wide="true">
      <slot></slot>
    </register-layout>
  </header>
</template>

<script>

import RegisterLayout from '../../frame/layout/Layout';

export default {
  name: 'functional-header',
  components: {
    RegisterLayout,
  },
  props: {
    modHasActions: {
      default: false,
      type: Boolean,
    },
  },
  data() {
    return {
      classes: {
        'functional-header': true,
        'functional-header--has-actions': this.modHasActions,
      },
    };
  },
};
</script>

<style scoped>
  .functional-header {
    margin-bottom: 0;
  }

  @media screen and (max-width: 767px) {
    .functional-header--has-actions:before, .functional-header__actions {
      display: block;
    }
  }
</style>
