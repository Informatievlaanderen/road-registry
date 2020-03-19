<template>
    <ul class="grid grid--is-stacked js-equal-height-container u-spacer">
      <slot></slot>
    </ul>
</template>

<script>
import RegistryLayout from '../layout/Layout'

export default {
  name: 'tile-grid',
};
</script>

<style scoped>
</style>
