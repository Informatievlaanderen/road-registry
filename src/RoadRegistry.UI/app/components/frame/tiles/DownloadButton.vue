<template>
    <li class="col--4-12 col--6-12--m col--12-12--xs">
      <a
        :class="{
          'not-allowed': this.downloading,
          'doormat' : true,
          'doormat--graphic' : true,
          'js-equal-height' : true,
          'paragraph--type--doormat-graphic': true,
          'paragraph--view-mode--default': true,
        }"
        @click="onClickHandle">
        <div class="doormat__graphic-wrapper">
        </div>
        <h2 class="doormat__title" >
          <span>{{title}}</span>
        </h2>
        <slot></slot>
        <div class="download-progress" v-if="downloading">
          <span class="progress" v-if="progress">{{downloadProgress_}}{{downloadProgressUnit}}</span>
          <div class="loader"/>
        </div>
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
    progress: {
      required: false,
      type: Number,
    },
    onClick: {
      required: true,
      type: Function,
    },
    downloading: {
      required: false,
      type: Boolean,
      default: false,
    },
  },
  data: function() {
    return {
      downloadProgress: 0,
      downloadUnit: '',
    }
  },
  computed: {
    downloadProgress_: function() {
        if (this.progress < 1024) {
        return this.progress;
      } else {
        return (this.progress > 10240)
          ? Math.round(this.progress / 1024)
          : Math.round(this.progress / 102.4) / 10;
      }
    },
    downloadProgressUnit: function() {
      return (this.progress < 1024) ? 'KB' : 'MB';
    },
  },
  methods:{
    onClickHandle(){
      if(false == this.downloading)
        this.onClick();

      return false;
    },
  },
};
</script>

<style scoped>
  a {
    cursor: pointer;
  }

  a.not-allowed {
    cursor: not-allowed;
  }

  /* todo align loader and downloaded amount */
  .download-progress {
    bottom: 2px;
    right: 2px;
    position: absolute;
  }

  .download-progress .loader {
    vertical-align: bottom;
  }

  .download-progress .progress {
    font-size: 0.75em;
    height: 1.8rem;
    color:#333 !important;
    vertical-align: bottom;
  }
</style>
