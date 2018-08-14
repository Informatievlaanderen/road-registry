<template>
  <div>
    <h1 class="h2 cta-title__title">Downloaden</h1>
    <tile-grid style="clear: both">
      <button-tile
        :on-click="clickDownloadRegistry"
        title="Register dump"
        :disabled=this.$store.getters.downloadFullRegistryInProcess>
        Download het volledige wegenregister als zip&#8209;bestand.
      </button-tile>
    </tile-grid>
  </div>
</template>

<script>
import TileGrid from '../../components/frame/tiles/Grid'
import ButtonTile from '../../components/frame/tiles/Button'
export default {
  components: {
    TileGrid,
    ButtonTile,
  },
  methods: {
    clickDownloadRegistry(){
      this.$store.dispatch('downloadRoadRegistery');
    }
  }
};
</script>
