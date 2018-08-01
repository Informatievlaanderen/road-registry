<template>
  <header :class="classes">
    <wr-layout :mod-is-wide="true">
      <slot></slot>
    </wr-layout>
  </header>
</template>

<script>

import WrLayout from '../../frame/layout/Layout';

export default {
  name: 'functional-header',
  components: {
    WrLayout,
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
