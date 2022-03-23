<template>
  <div>
    <h2>Go to pages:</h2>
    <ul>
      <li>
        <router-link :to="`/activiteit`">Activiteit</router-link>
      </li>
      <li>
       <router-link :to="`/informatie`">Informatie</router-link>
      </li>
      <li>
       <router-link :to="`/download-extract`">Download Extract</router-link>
      </li>
      <li>
       <router-link :to="`/download-product`">Download Product</router-link>
      </li>
      <li>
       <router-link :to="`/uploads`">Uploads</router-link>
      </li>
    </ul>
  </div>
</template>