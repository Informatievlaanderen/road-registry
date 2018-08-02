<template>
    <li class="col--4-12 col--6-12--m col--12-12--xs">
      <a
        class="doormat doormat--graphic js-equal-height paragraph--type--doormat-graphic paragraph--view-mode--default"
        :class="classes"
        @click="onClickHandle">
        <div class="doormat__graphic-wrapper">
        </div>
        <h2 class="doormat__title" data-clamp="2">
          <span>{{title}}</span>
        </h2>
        <slot></slot>
      </a>
    </li>
</template>

<script>

export default {
  name: 'tile-button',
  props: {
    title: {
      required: true,
      type: String,
    },
    onClick: {
      required: true,
      type: Function,
    },
    disabled: {
      required: false,
      type: Boolean,
      default: false,
    }
  },
  methods:{
    onClickHandle(){
      if(this.disabled)
        return false;

      this.onClick();
      return false;
    }
  },
  data (){
    return {
      classes: {
        'not-allowed': disabled,
      }
    }
  }

};
</script>

<style scoped>
  a {
    cursor: pointer;
  }

  a.not-allowed {
    cursor: not-allowed;
  }
</style>
