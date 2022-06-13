<template>
<h1>Afmelden</h1>
</template>

<script lang="ts">
import Vue from "vue"
import { AuthService } from "@/services/auth-service"

export default Vue.extend({
    data() {
        return {
            isLogoutInProgress: false,
        };
    },
    props: {
        redirect: String
    },
    methods: {
        logout() {
            this.isLogoutInProgress = true;
            AuthService.logout();
            this.isLogoutInProgress = false;
        }
    },
})
</script>