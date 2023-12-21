<template>
  <div>
    <div v-if="activities.length === 0">
      <vl-region>
        <div v-vl-align:center>
          <vl-loader message="Uw pagina is aan het laden" />
        </div>
      </vl-region>
    </div>
    <div v-else>
      <div class="vl-steps vl-steps--timeline">
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
                        <vl-button v-if="isDownloading" mod-loading> Download... </vl-button>
                        <vl-button v-else @click="downloadExtract(activity)"> Download </vl-button>
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
                        <div class="vl-grid vl-grid--align-center grid-summary">
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>
                                  {{ activity.changeFeedContent.content.summary.roadNodes.added }}
                                </div>
                                <div class="vl-infotext__text">Toegevoegde wegknopen</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>
                                  {{ activity.changeFeedContent.content.summary.roadNodes.modified }}
                                </div>
                                <div class="vl-infotext__text">Gewijzigde wegknopen</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>
                                  {{ activity.changeFeedContent.content.summary.roadNodes.removed }}
                                </div>
                                <div class="vl-infotext__text">Verwijderde wegknopen</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>
                                  {{ activity.changeFeedContent.content.summary.roadSegments.added }}
                                </div>
                                <div class="vl-infotext__text">Toegevoegde wegsegmenten</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>
                                  {{ activity.changeFeedContent.content.summary.roadSegments.modified }}
                                </div>
                                <div class="vl-infotext__text">Gewijzigde wegsegmenten</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>
                                  {{ activity.changeFeedContent.content.summary.roadSegments.removed }}
                                </div>
                                <div class="vl-infotext__text">Verwijderde wegsegmenten</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>
                                  {{ activity.changeFeedContent.content.summary.gradeSeparatedJunctions.added }}
                                </div>
                                <div class="vl-infotext__text">Toegevoegde ongelijkgrondse kruisingen</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>
                                  {{ activity.changeFeedContent.content.summary.gradeSeparatedJunctions.modified }}
                                </div>
                                <div class="vl-infotext__text">Gewijzigde ongelijkgrondse kruisingen</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>
                                  {{ activity.changeFeedContent.content.summary.gradeSeparatedJunctions.removed }}
                                </div>
                                <div class="vl-infotext__text">Verwijderde ongelijkgrondse kruisingen</div>
                              </div>
                            </div>
                          </div>
                        </div>
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
            <vl-button v-else v-on:click="loadNextPage()">Meer ...</vl-button>
          </div>
        </vl-column>
      </vl-grid>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { BackOfficeApi, PublicApi } from "../../../services";
import RoadRegistry from "../../../types/road-registry";
import ActivityProblems from "../components/ActivityProblems.vue";

export default Vue.extend({
  components: {
    ActivityProblems,
  },
  data() {
    return {
      activities: [] as Activity[],
      pagination: {
        pageSize: 25,
        isLoading: false,
        isLoadingTop: false,
      },
      isDownloading: false,
      autoRefreshInterval: null as any,
    };
  },
  async mounted() {
    await this.loadToTop();
    this.autoRefreshInterval = setInterval(this.loadToTop, 10000);
  },
  beforeDestroy() {
    if (this.autoRefreshInterval) {
      clearInterval(this.autoRefreshInterval);
    }
  },
  methods: {
    async loadToTop(): Promise<any> {
      if (this.pagination.isLoadingTop) {
        console.warn("Skipping load, loading is still in progress");
        return;
      }

      this.pagination.isLoadingTop = true;
      try {
        let response = await PublicApi.ChangeFeed.getHead(this.pagination.pageSize);
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
      } finally {
        this.pagination.isLoadingTop = false;
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

        var response = await PublicApi.ChangeFeed.getPrevious(currentEntry, this.pagination.pageSize);
        this.activities = this.activities.concat(response.entries.map((entry) => new Activity(entry)));
      } finally {
        this.pagination.isLoading = false;
      }
    },
    async downloadUpload(activity: any): Promise<void> {
      this.isDownloading = true;
      try {
        await BackOfficeApi.Uploads.download(activity.changeFeedContent.content.archive.id);
      } finally {
        this.isDownloading = false;
      }
    },
    async downloadExtract(activity: any): Promise<void> {
      this.isDownloading = true;
      try {
        await BackOfficeApi.Extracts.download(activity.changeFeedContent.content.archive.id);
      } finally {
        this.isDownloading = false;
      }
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
