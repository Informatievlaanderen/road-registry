<template>
  <div>
    <h1 class="h2 cta-title__title">Downloaden</h1>
    <tile-grid style="clear: both">
      <download-button-tile
        :on-click="clickDownloadRegistry"
        title="Register dump"
        :progress=downloadFullRegistryProcess
        :downloading=downloadFullRegistryInProcess>
        Download het volledige wegenregister als zip&#8209;bestand.
      </download-button-tile>
    </tile-grid>
  </div>
</template>

<script>
import TileGrid from '../../components/frame/tiles/Grid'
import DownloadButtonTile from '../../components/frame/tiles/DownloadButton'
import { mapGetters } from 'vuex';
export default {
  components: {
    TileGrid,
    DownloadButtonTile,
  },
  methods: {
    clickDownloadRegistry(){
      this.$store.dispatch('downloadRoadRegistery');
    }
  },
  computed: {
    ...mapGetters({
      downloadFullRegistryProcess: 'downloadFullRegistryProcess',
      downloadFullRegistryInProcess: 'downloadFullRegistryInProcess',
    }),
  },
};
</script>
