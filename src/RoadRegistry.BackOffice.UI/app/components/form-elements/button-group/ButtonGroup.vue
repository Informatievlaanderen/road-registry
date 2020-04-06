<template>
  <div :class="classes">
    <slot></slot>
  </div>
</template>

<script>
export default {
  name: 'button-group',
  props: {
    modIsRight: {
      default: false,
      type: Boolean
    }
  },
  data () {
    return {
      classes: {
        'button-group': true,
        'button-group--right': this.modIsRight
      }
    }
  }
}
</script>
