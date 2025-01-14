<template>
  <div>
    <div v-if="!firstLoadCompleted">
      <vl-region>
        <div v-vl-align:center>
          <vl-loader message="Uw pagina is aan het laden" />
        </div>
      </vl-region>
    </div>
    <div v-else>
      <vl-grid mod-stacked>
        <vl-column>
          <div class="vl-form-col--12-12">
            <input
              type="text"
              class="vl-input-field vl-input-field--block"
              v-model="filter"
              placeholder="Filter"
              @input="onFilterInput"
              @keyup.esc="resetFilter()"
            />
          </div>
        </vl-column>
      </vl-grid>

      <div v-if="isWaitingForFirstFilterResult">
        <vl-region>
          <div v-vl-align:center>
            <vl-loader message="Even geduld a.u.b." />
          </div>
        </vl-region>
      </div>
      <div v-else class="vl-steps vl-steps--timeline">
        <ul class="vl-steps__list">
          <li class="vl-step vl-step" v-for="activity in activities" :key="activity.id">
            <div class="vl-step__container">
              <div class="vl-step__icon">
                <span class="vl-step__icon__sub">{{ activity.changeFeedEntry.day }}</span>
                <span class="vl-step__icon__sub">{{ activity.changeFeedEntry.month }}</span>
                <span class="vl-step__icon__sub">{{ activity.changeFeedEntry.timeOfDay }}</span>
              </div>
              <div class="vl-step__wrapper">
                <div class="vl-layout">
                  <div class="vl-grid">
                    <div class="vl-col--11-12">
                      <h3 class="vl-step__title">
                        {{ activity.changeFeedEntry.title }}
                        <div
                          style="background-color: red"
                          v-if="activity.changeFeedEntry.type === 'RoadNetworkChangesBasedOnArchiveAccepted'"
                        >
                          {{ activity.changeFeedEntry.type }}
                        </div>
                      </h3>
                    </div>
                    <div class="vl-col--1-12 vl-u-align-right">
                      <vl-button
                        :icon="activity.iconSelector"
                        mod-icon
                        mod-naked
                        v-if="activity.hasContent()"
                        v-on:click="activity.toggleContentVisibility()"
                      ></vl-button>
                    </div>
                  </div>
                </div>
                <div class="vl-step__content-wrapper" v-if="activity.isContentVisible">
                  <div class="vl-step__content">
                    <div v-if="activity.changeFeedContent">
                      <br />
                      <div
                        v-if="
                          activity.changeFeedContent.content.archive.id &&
                          [
                            'RoadNetworkChangesArchiveUploaded',
                            'RoadNetworkExtractChangesArchiveUploaded',
                            'NoRoadNetworkChanges',
                          ].some((x) => x === activity.changeFeedEntry.type)
                        "
                      >
                        <vl-button v-if="isDownloading" mod-loading> Download... </vl-button>
                        <vl-button v-else @click="downloadUpload(activity)"> Download </vl-button>
                      </div>
                      <div
                        v-else-if="
                          activity.changeFeedContent.content.archive.id &&
                          ['RoadNetworkExtractDownloadBecameAvailable'].some((x) => x === activity.changeFeedEntry.type)
                        "
                      >
                        <div v-if="activity.changeFeedContent.content?.overlapsWithDownloadIds?.length > 0">
                          <vl-alert icon="warning" mod-warning mod-small>
                            De contour voor deze extractaanvraag overlapt met de contour van een andere, open
                            extractaanvraag.
                          </vl-alert>
                          <br />
                        </div>
                        <div>
                          <vl-button v-if="isDownloading" mod-loading> Download... </vl-button>
                          <vl-button v-else @click="downloadExtract(activity)"> Download </vl-button>
                        </div>
                      </div>

                      <div
                        v-else-if="
                          [
                            'RoadNetworkChangesArchiveRejected',
                            'RoadNetworkChangesArchiveAccepted',
                            'RoadNetworkExtractChangesArchiveAccepted',
                            'RoadNetworkExtractChangesArchiveRejected',
                          ].some((x) => x === activity.changeFeedEntry.type)
                        "
                      >
                        <div v-for="file in activity.changeFeedContent.content.files" :key="file.file">
                          <h3>
                            <strong>{{ file.file }}</strong>
                          </h3>
                          <h3 v-if="file.change">
                            <strong>{{ file.change }}</strong>
                          </h3>
                          <ActivityProblems :problems="file.problems" />
                          <br />
                        </div>
                        <div v-if="activity.changeFeedContent.content.archive.id">
                          <vl-button v-if="isDownloading" mod-loading> Download... </vl-button>
                          <vl-button v-else @click="downloadUpload(activity)"> Download </vl-button>
                        </div>
                      </div>
                      <div
                        v-else-if="['RoadNetworkChangesAccepted:v2'].some((x) => x === activity.changeFeedEntry.type)"
                      >
                        <ActivitySummary :summary="activity.changeFeedContent.content.summary" />

                        <div v-for="change in activity.changeFeedContent.content.changes" :key="change.change">
                          <h3>
                            <strong>{{ change.change }}</strong>
                          </h3>
                          <ActivityProblems :problems="change.problems" />
                          <br />
                        </div>
                        <div v-if="activity.changeFeedContent.content.archive.id">
                          <vl-button v-if="isDownloading" mod-loading> Download... </vl-button>
                          <vl-button v-else @click="downloadUpload(activity)"> Download </vl-button>
                        </div>
                      </div>
                      <div v-else-if="['RoadNetworkChangesRejected'].some((x) => x === activity.changeFeedEntry.type)">
                        <div v-for="change in activity.changeFeedContent.content.changes" :key="change.change">
                          <h3>
                            <strong>{{ change.change }}</strong>
                          </h3>
                          <ActivityProblems :problems="change.problems" />
                          <br />
                        </div>
                        <div v-if="activity.changeFeedContent.content.archive.id">
                          <vl-button v-if="isDownloading" mod-loading> Download... </vl-button>
                          <vl-button v-else @click="downloadUpload(activity)"> Download </vl-button>
                        </div>
                      </div>
                    </div>
                    <div v-else>
                      <div class="vl-skeleton">
                        <div class="vl-skeleton-bone vl-skeleton-bone--body-text">
                          <span class="vl-u-visually-hidden">Text is loading...</span>
                        </div>
                        <div class="vl-skeleton-bone vl-skeleton-bone--body-text">
                          <span class="vl-u-visually-hidden">Text is loading...</span>
                        </div>
                        <div class="vl-skeleton-bone vl-skeleton-bone--body-text">
                          <span class="vl-u-visually-hidden">Text is loading...</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </li>
        </ul>
      </div>

      <vl-grid mod-stacked>
        <vl-column>
          <div v-vl-flex v-vl-flex:align-center>
            <vl-button mod-loading v-if="pagination.isLoading"></vl-button>
            <vl-button v-else-if="activities.length >= pagination.pageSize" v-on:click="loadNextPage()">
              Meer ...
            </vl-button>
          </div>
        </vl-column>
      </vl-grid>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { debounce } from "lodash";
import { BackOfficeApi, PublicApi } from "../../../services";
import RoadRegistry from "../../../types/road-registry";
import ActivityProblems from "../components/ActivityProblems.vue";
import ActivitySummary from "../components/ActivitySummary.vue";

export default Vue.extend({
  components: {
    ActivityProblems,
    ActivitySummary,
  },
  data() {
    return {
      activities: [] as Activity[],
      firstLoadCompleted: false,
      filter: "",
      isWaitingForFirstFilterResult: false,
      pagination: {
        pageSize: 25,
        isLoading: false,
        isLoadingTop: false,
      },
      isDownloading: false,
      autoRefreshInterval: null as any,
      debouncedLoadToTop: () => {},
    };
  },
  created() {
    this.debouncedLoadToTop = debounce(this.loadToTop, 500, { trailing: true });
  },
  mounted() {
    this.filter = (this.$route.query.filter ?? "").toString();
    this.loadToTop();
  },
  beforeDestroy() {
    this.stopAutoRefresh();
  },
  methods: {
    startAutoRefresh() {
      this.autoRefreshInterval = setInterval(this.loadToTop, 10000);
    },
    stopAutoRefresh() {
      if (this.autoRefreshInterval) {
        clearInterval(this.autoRefreshInterval);
      }
    },
    resetFilter() {
      this.filter = "";
      this.onFilterInput();
    },
    onFilterInput() {
      this.isWaitingForFirstFilterResult = true;
      this.stopAutoRefresh();
      this.setQueryParams({ filter: this.filter });
      this.resetActivities();

      this.debouncedLoadToTop();
    },
    resetActivities() {
      this.activities = [];
    },
    async loadToTop(): Promise<any> {
      if (this.pagination.isLoadingTop) {
        console.warn("Skipping load, loading is still in progress");
        return;
      }

      this.stopAutoRefresh();
      this.pagination.isLoadingTop = true;
      try {
        let response = await PublicApi.ChangeFeed.getHead(this.pagination.pageSize, this.filter);
        let activities = response.entries.map((entry) => new Activity(entry));

        if (this.activities.length) {
          let firstActivityId = this.activities[0].id;

          let firstActivityInReceivedData = activities.find((x) => x.id === firstActivityId);
          if (firstActivityInReceivedData) {
            let index = activities.indexOf(firstActivityInReceivedData);
            activities = activities.slice(0, index);
            if (!activities.length) {
              return;
            }
          } else {
            // current data is too old, do a refresh
            this.activities = [];
          }
        }

        this.activities = [...activities, ...this.activities];
        this.firstLoadCompleted = true;
      } finally {
        this.pagination.isLoadingTop = false;
        this.isWaitingForFirstFilterResult = false;
        this.startAutoRefresh();
      }
    },
    async loadNextPage(): Promise<any> {
      if (this.pagination.isLoading) {
        console.warn("Skipping load, loading is still in progress");
        return;
      }

      this.pagination.isLoading = true;
      try {
        const currentEntry = Math.min(...this.activities.map((a) => a.id));

        var response = await PublicApi.ChangeFeed.getPrevious(currentEntry, this.pagination.pageSize, this.filter);
        this.activities = this.activities.concat(response.entries.map((entry) => new Activity(entry)));
      } finally {
        this.pagination.isLoading = false;
      }
    },
    async downloadUpload(activity: any): Promise<void> {
      this.isDownloading = true;
      try {
        PublicApi.Uploads.downloadUsingPresignedUrl(activity.changeFeedContent.content.archive.id);
      } finally {
        this.isDownloading = false;
      }
    },
    async downloadExtract(activity: any): Promise<void> {
      this.isDownloading = true;
      try {
        await PublicApi.Extracts.downloadUsingPresignedUrl(activity.changeFeedContent.content.archive.id);
      } finally {
        this.isDownloading = false;
      }
    },
    setQueryParams(params: any) {
      history.pushState(
        {},
        "",
        this.$route.path +
          "?" +
          Object.keys(params)
            .filter((key) => params[key])
            .map((key) => {
              return encodeURIComponent(key) + "=" + encodeURIComponent(params[key]);
            })
            .join("&")
      );
    },
  },
});

class Activity {
  _changeFeedEntry: RoadRegistry.ChangeFeedEntry;
  _lazyChangeFeedContent?: RoadRegistry.ChangeFeedContent;
  _isChangeFeedContentVisible: boolean = false;

  constructor(changeFeedEntry: RoadRegistry.ChangeFeedEntry) {
    this._changeFeedEntry = changeFeedEntry;
  }

  public get changeFeedEntry(): RoadRegistry.ChangeFeedEntry {
    return this._changeFeedEntry;
  }

  public get id(): number {
    return this._changeFeedEntry.id;
  }

  public get changeFeedContent(): RoadRegistry.ChangeFeedContent | undefined {
    return this._lazyChangeFeedContent;
  }

  public get isContentVisible(): boolean {
    return this._isChangeFeedContentVisible;
  }

  public async toggleContentVisibility(): Promise<any> {
    if (this.hasContent()) {
      this._isChangeFeedContentVisible = !this._isChangeFeedContentVisible;

      if (this._lazyChangeFeedContent == null) {
        this._lazyChangeFeedContent = await this.getChangeFeedContent();
      }
    }
  }

  public get iconSelector(): string {
    if (this._isChangeFeedContentVisible) {
      return "minus";
    } else {
      return "plus";
    }
  }

  public hasContent(): boolean {
    return [
      "RoadNetworkChangesArchiveUploaded",
      "RoadNetworkChangesArchiveRejected",
      "RoadNetworkChangesArchiveAccepted",
      "RoadNetworkChangesAccepted:v2",
      "RoadNetworkChangesRejected",
      "RoadNetworkExtractChangesArchiveAccepted",
      "RoadNetworkExtractChangesArchiveUploaded",
      "RoadNetworkExtractDownloadBecameAvailable",
      "RoadNetworkExtractChangesArchiveRejected",
      "NoRoadNetworkChanges",
    ].some((type) => type === this._changeFeedEntry.type);
  }

  private getChangeFeedContent(): Promise<RoadRegistry.ChangeFeedContent> {
    return PublicApi.ChangeFeed.getContent(this.id);
  }
}
</script>

<style lang="scss" scoped>
.vl-step__wrapper {
  border: 1px solid #ccc;
  padding: 1.5rem;
  margin-left: -1.5rem;
}

.grid-summary {
  background-color: #e8ebee;
  margin-bottom: 5px;
  margin-left: 0px;
  padding-bottom: 10px;
}

.vl-steps {
  margin-top: 1rem;
}

.vl-steps--timeline {
  .vl-step__container::after {
    top: 6rem;
  }

  .vl-step__icon {
    height: 6rem;

    .vl-step__icon__sub {
      font-weight: 500;
      font-size: 1.8rem;
    }
  }
}
</style>
