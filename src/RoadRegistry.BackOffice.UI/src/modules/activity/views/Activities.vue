<template>
  <div>
    {{ $route.params.downloadId }}
    <div v-if="activities.length === 0">
      <div class="vl-col--1-1">
        <div class="vl-region">
          <div class="vl-u-align-center">
            <div class="vl-loader" role="status"></div>
            <p>Pagina is aan het laden</p>
          </div>
        </div>
      </div>
    </div>
    <div v-else>
      <div class="vl-steps vl-steps--timeline">
        <ul class="vl-steps__list">
          <li class="vl-step vl-step" v-for="activity in activities" :key="activity.id">
            <div class="vl-step__container">
              <div class="vl-step__icon">
                {{ activity.changeFeedEntry.day }}
                <span class="vl-step__icon__sub">{{ activity.changeFeedEntry.month }}</span>
                <span class="vl-step__icon__sub">{{ activity.changeFeedEntry.timeOfDay }}</span>
              </div>
              <div class="vl-step__wrapper">
                <div class="vl-layout">
                  <div class="vl-grid">
                    <div class="vl-col--11-12">
                      <h3 class="vl-step__title">
                        <!-- <a href="#" v-on:click="activity.toggleContentVisibility()">{{ activity.changeFeedEntry.title }} {{activity.id}}</a> -->
                        {{ activity.changeFeedEntry.title }}
                        <div style="background-color: red;" v-if="activity.changeFeedEntry.type === 'RoadNetworkChangesBasedOnArchiveAccepted' ">
                          {{ activity.changeFeedEntry.type }}
                        </div>
                        </h3>
                    </div>
                    <div class="vl-col--1-12 vl-u-align-right">
                      <div v-if="activity.hasContent()">
                        <button class="vl-button vl-button--naked-action vl-button--icon vl-button--large" vl-button v-on:click="activity.toggleContentVisibility()">
                          <i class="vl-button__icon vl-vi" aria-hidden="true" v-bind:class="{ 'vl-vi-minus': activity.isContentVisible, 'vl-vi-plus': !activity.isContentVisible}"></i>
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
                <div class="vl-step__content-wrapper" v-if="activity.isContentVisible">
                  <div class="vl-step__content">
                    <div v-if="activity.changeFeedContent">
                      <div v-if="['RoadNetworkChangesArchiveUploaded', 'RoadNetworkExtractChangesArchiveUploaded', 'NoRoadNetworkChanges'].some(x => x === activity.changeFeedEntry.type)">
                        Archief: 
                        <a :href="'/roads/v1/upload/' + activity.changeFeedContent.content.archive.id" target="_blank">
                          Download {{activity.changeFeedContent.content.archive.filename}}
                        </a>
                      </div>
                      <div v-else-if="['RoadNetworkExtractDownloadBecameAvailable'].some(x => x === activity.changeFeedEntry.type)">
                        Archief: 
                        <a :href="'/roads/v1/extracts/download/' + activity.changeFeedContent.content.archive.id" target="_blank">
                          Download {{activity.changeFeedContent.content.archive.filename}}
                        </a>
                      </div>
                      <div v-else-if="['RoadNetworkChangesArchiveRejected', 'RoadNetworkChangesArchiveAccepted', 'RoadNetworkExtractChangesArchiveAccepted', 'RoadNetworkExtractChangesArchiveRejected'].some(x => x === activity.changeFeedEntry.type)">
                        <ul>
                          <li v-for="file in activity.changeFeedContent.content.files" :key="file.file">
                            <p><strong>{{file.file}}</strong></p>
                            <ul class="vl-icon-list">
                              <li class="vl-icon-list__item" v-for="problem in file.problems" :key="problem.text">
                                <abbr title="Error" v-if="problem.severity == 'Error'">
                                  <i class="vl-vi vl-vi-alert-triangle" aria-hidden="true"></i>
                                </abbr>
                                <abbr title="Waarschuwing" v-if="problem.severity == 'Warning'">
                                  <i class="vl-vi vl-vi-alert-small" aria-hidden="true"></i>
                                </abbr>
                                {{ problem.text }}
                              </li>
                            </ul>
                          </li>
                        </ul>
                        <p>
                          <a :href="'/roads/v1/upload/' + activity.changeFeedContent.content.archive.id" target="_blank">
                            Download {{activity.changeFeedContent.content.archive.filename}}
                          </a>
                        </p>
                      </div>
                      <div v-else-if="['RoadNetworkChangesAccepted:v2'].some(x => x === activity.changeFeedEntry.type)">
                      <!-- <div v-if="1 == 1"> -->
                        <div class="vl-grid vl-grid--align-center grid-summary">
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>{{ activity.changeFeedContent.content.summary.roadNodes.added }}</div>
                                <div class="vl-infotext__text">Toegevoegde wegknopen</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>{{ activity.changeFeedContent.content.summary.roadNodes.modified }}</div>
                                <div class="vl-infotext__text">Gewijzigde wegknopen</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>{{ activity.changeFeedContent.content.summary.roadNodes.removed }}</div>
                                <div class="vl-infotext__text">Verwijderde wegknopen</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>{{ activity.changeFeedContent.content.summary.roadSegments.added }}</div>
                                <div class="vl-infotext__text">Toegevoegde wegsegmenten</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>{{ activity.changeFeedContent.content.summary.roadSegments.modified }}</div>
                                <div class="vl-infotext__text">Gewijzigde wegsegmenten</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>{{ activity.changeFeedContent.content.summary.roadSegments.removed }}</div>
                                <div class="vl-infotext__text">Verwijderde wegsegmenten</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>{{ activity.changeFeedContent.content.summary.gradeSeparatedJunctions.added }}</div>
                                <div class="vl-infotext__text">Toegevoegde ongelijkgrondse kruisingen</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>{{ activity.changeFeedContent.content.summary.gradeSeparatedJunctions.modified }}</div>
                                <div class="vl-infotext__text">Gewijzigde ongelijkgrondse kruisingen</div>
                              </div>
                            </div>
                          </div>
                          <div class="vl-col--4-12 vl-u-align-center">
                            <div class="vl-infotext-wrapper">
                              <div class="vl-infotext">
                                <div class="vl-infotext__value" data-vl-infotext-value>{{ activity.changeFeedContent.content.summary.gradeSeparatedJunctions.removed }}</div>
                                <div class="vl-infotext__text">Verwijderde ongelijkgrondse kruisingen</div>
                              </div>
                            </div>
                          </div>
                        </div>
                        <ul>
                          <li v-for="change in activity.changeFeedContent.content.changes" :key="change.change">
                            <p><strong>{{change.change}}</strong></p>
                            <ul class="vl-icon-list">
                              <li class="vl-icon-list__item" v-for="problem in change.problems" :key="problem.text">
                                <abbr title="Error" v-if="problem.severity == 'Error'">
                                  <i class="vl-vi vl-vi-alert-triangle" aria-hidden="true"></i>
                                </abbr>
                                <abbr title="Waarschuwing" v-if="problem.severity == 'Warning'">
                                  <i class="vl-vi vl-vi-alert-small" aria-hidden="true"></i>
                                </abbr>
                                {{ problem.text }}
                              </li>
                            </ul>
                          </li>
                        </ul>
                        <p>
                          <a :href="'/roads/v1/upload/' + activity.changeFeedContent.content.archive.id" target="_blank">
                            Download {{activity.changeFeedContent.content.archive.filename}}
                          </a>
                        </p>
                      </div>
                      <div v-else-if="['RoadNetworkChangesRejected'].some(x => x === activity.changeFeedEntry.type)">
                        <ul>
                          <li v-for="change in activity.changeFeedContent.content.changes" :key="change.change">
                            <p><strong>{{change.change}}</strong></p>
                            <ul class="vl-icon-list">
                              <li class="vl-icon-list__item" v-for="problem in change.problems" :key="problem.text">
                                <abbr title="Error" v-if="problem.severity == 'Error'">
                                  <i class="vl-vi vl-vi-alert-triangle" aria-hidden="true"></i>
                                </abbr>
                                <abbr title="Waarschuwing" v-if="problem.severity == 'Warning'">
                                  <i class="vl-vi vl-vi-alert-small" aria-hidden="true"></i>
                                </abbr>
                                {{ problem.text }}
                              </li>
                            </ul>
                          </li>
                        </ul>
                        <p>
                          <a :href="'/roads/v1/upload/' + activity.changeFeedContent.content.archive.id" target="_blank">
                            Download {{activity.changeFeedContent.content.archive.filename}}
                          </a>
                        </p>
                      </div>
                      <div v-else>
                        {{ activity.changeFeedContent }}
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

      <section class="vl-region">
        <div class="vl-layout">
          <div class="vl-grid vl-grid--align-center">
            <div class="vl-col--6-12 vl-u-align-center">
              <button
                class="vl-button vl-button--wide"
                vl-button
                v-on:click="loadNextPage()"
                v-bind:class="{ 'vl-button--disabled': pagination.isLoading }"
                :disabled="pagination.isLoading">
                <span class="vl-button__label vl-loader vl-loader--light" v-if="pagination.isLoading"></span>
                <span class="vl-button__label" v-else>Meer &hellip;</span>
              </button>
            </div>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { PublicApi } from "../../../services";
import RoadRegistry from "../../../types/road-registry";

export default Vue.extend({
  data() {
    return {
      activities: [] as Activity[],
      pagination: {
        pageSize: 25,
        isLoading: false,
      },
    };
  },
  async mounted() {
    var response = await PublicApi.ChangeFeed.getHead(this.pagination.pageSize);
    this.activities = response.entries.map((entry) => new Activity(entry));
  },
  methods: {
    async loadNextPage(): Promise<any> {
      this.pagination.isLoading = true;
      const currentEntry = Math.min(...this.activities.map((a) => a.id));

      var response = await PublicApi.ChangeFeed.getPrevious(currentEntry, this.pagination.pageSize);
      this.activities = this.activities.concat(response.entries.map((entry) => new Activity(entry)));
      this.pagination.isLoading = false;
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

  public hasContent(): boolean {
    return [
      'RoadNetworkChangesArchiveUploaded',
      'RoadNetworkChangesArchiveRejected',
      'RoadNetworkChangesArchiveAccepted',
      'RoadNetworkChangesAccepted:v2',
      'RoadNetworkChangesRejected',
      'RoadNetworkExtractChangesArchiveAccepted',
      'RoadNetworkExtractChangesArchiveUploaded',
      'RoadNetworkExtractDownloadBecameAvailable',
      'RoadNetworkExtractChangesArchiveRejected',
      'NoRoadNetworkChanges'
    ].some(type => type === this._changeFeedEntry.type);
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
</style>
