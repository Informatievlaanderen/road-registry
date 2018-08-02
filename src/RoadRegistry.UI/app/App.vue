<template>
  <div>
    <registery-header />
    <functional-header mod-has-actions>
      <functional-header-actions>
        <functional-header-action title="Sam Admin (Beheerder | Ontwikkelaar)" />
        <functional-header-action title="Afmelden" :on="logoutClicked" />
      </functional-header-actions>

      <functional-header-content>
        <router-link slot="title" class="functional-header__title" :to="{ name: 'downloads' }">{{ title }}</router-link>
      </functional-header-content>

      <functional-header-sub>
        <registery-grid>
          <registery-column type="nav"
                     role="navigation"
                     :cols="[{nom: 9, den: 12}, {nom: 8, den: 12, mod: 's'}, {nom: 1, den: 1, mod: 's'}]"
                     data-tabs-responsive-label="Navigatie">
            <header-tabs mod-is-functional-header>
              <header-tab title="Downloaden" exact :to="{ name: 'downloads' }" />
              <header-tab title="Opladen" :to="{ name: 'uploads' }" />
            </header-tabs>
          </registery-column>
        </registery-grid>
      </functional-header-sub>
    </functional-header>

    <registery-main>
      <registery-region>
        <registery-layout mod-is-wide>
          <registery-alert
            :title="alert.title"
            :type="alert.type"
            :visible="alert.visible">
              {{alert.content}}
            </registery-alert>
          <router-view></router-view>
        </registery-layout>
      </registery-region>
    </registery-main>

    <registery-footer />
  </div>
</template>

<script>
import RegisteryHeader from 'components/partials/header/Header';

import FunctionalHeader from 'components/partials/functional-header/FunctionalHeader';
import FunctionalHeaderAction from 'components/partials/functional-header/FunctionalHeaderAction';
import FunctionalHeaderActions from 'components/partials/functional-header/FunctionalHeaderActions';
import FunctionalHeaderContent from 'components/partials/functional-header/FunctionalHeaderContent';
import FunctionalHeaderSub from 'components/partials/functional-header/FunctionalHeaderSub';

import RegisteryGrid from 'components/frame/grid/Grid';
import RegisteryColumn from 'components/frame/column/Column';
import HeaderTabs from 'components/navigations/tabs/Tabs';
import HeaderTab from 'components/navigations/tabs/Tab';

import RegisteryMain from 'components/frame/main/Main';
import RegisteryRegion from 'components/frame/region/Region';
import RegisteryLayout from 'components/frame/layout/Layout';

import RegisteryFooter from 'components/partials/footer/Footer';

import RegisteryAlert from 'components/partials/alert/Alert';

import { mapGetters } from 'vuex';

export default {
  name: 'App',
  components: {
    RegisteryHeader,
    FunctionalHeader,
    FunctionalHeaderActions,
    FunctionalHeaderAction,
    FunctionalHeaderContent,
    FunctionalHeaderSub,
    RegisteryGrid,
    RegisteryColumn,
    HeaderTab,
    HeaderTabs,
    RegisteryMain,
    RegisteryRegion,
    RegisteryLayout,
    RegisteryFooter,
    RegisteryAlert,
  },
  computed: {
    ...mapGetters({
      alert: 'alert',
    }),
  },
  methods: {
    logoutClicked() {
      // eslint-disable-next-line
      console.log('logout clicked');
    },
  },
  data() {
    return {
      title: 'WEGENREGISTER',
    };
  },
};
</script>

<style>
a:before,
a:hover:before { text-decoration: none; }

.properties--disabled {
  color: #cbd2da;
  font-weight: lighter;
}
</style>
