<template>
  <div>
    <h1><strong>Authenticatie</strong></h1>
    <input id="apiKey" type="text" />
    <p>Om gebruik te kunnen maken van onze diensten hebt u een API sleutel nodig.</p>
    <p>
      Indien u niet beschikt over een geldige sleutel kunt u contact opnemen met uw domeinverantwoordelijke of een
      beheerder van het wegenregister.
    </p>
    <vl-action-group>
      <vl-button v-if="isLoggingIn" mod-disabled>Aanmelden</vl-button>
      <vl-button v-else @click="login">Aanmelden</vl-button>
    </vl-action-group>
  </div>
</template>
