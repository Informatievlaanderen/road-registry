<template>
  <div>
    <input
      type="text"
      :name="name"
      :id="id"
      :class="{
          'input-field': true,
          'input-field--block': this.modIsBlock,
          'input-field--small': this.modIsSmall,
          'input-field--disabled': this.disabled,
          'input-required': this.hasDataIsMissingClass
        }"
      :disabled="disabled"
      :value="inputValue"
      :placeholder="placeholder"
      v-bind="attributes"
      v-validate="validation"
      @input="onInput" />
  </div>
</template>

<script>
import {
  SET_REQUIRED_FIELD_STATUS,
  REMOVE_REQUIRED_FIELD_STATUS,
} from 'store/mutation-types';

export default {
  inject: ['$validator'],
  name: 'input-field',
  props: {
    name: {
      default: '',
      type: String,
    },
    value: {
      default: '',
      type: String,
    },
    id: {
      default: '',
      type: String,
    },
    placeholder: {
      default: '',
      type: String,
    },
    disabled: {
      default: false,
      type: Boolean,
    },
    modIsBlock: {
      default: true,
      type: Boolean,
    },
    modIsSmall: {
      default: false,
      type: Boolean,
    },
    modIsError: {
      default: false,
      type: Boolean,
    },
    validation: {
      default: '',
      type: String,
    },
  },
  computed: {
    isRequired() {
      return (this.validation || '').toLowerCase().includes('required');
    },
    hasDataIsMissingClass() {
      return this.isRequired && !this.inputValue;
    },
  },
  data() {
    return {
      attributes: {},
      inputValue: '',
      requiredFieldPushedToStore: false,
    };
  },
  methods: {
    onInput(event){
      const { target = {} } = event;
      this.inputValue = target.value;
    },
    pushRequiredStatus(){
      if(this.isRequired){
        const payload = {
          name: this.name,
          isEmpty: !this.inputValue,
        };
        this.$store.commit(SET_REQUIRED_FIELD_STATUS, payload);
        this.requiredFieldPushedToStore = true;
      } else {
        // avoid unnecessary REMOVE_REQUIRED_FIELD_STATUS mutations
        if(this.requiredFieldPushedToStore){
          this.$store.commit(REMOVE_REQUIRED_FIELD_STATUS, this.name);
          this.requiredFieldPushedToStore = false;
        }
      }
    }
  },
  mounted(){
    this.inputValue = this.value;
    this.pushRequiredStatus();
  },
  watch: {
    value(updatedValue, previousValue) {
      this.inputValue = updatedValue;
    },
    isRequired(isRequired, wasRequired){
      if(isRequired !== wasRequired){
        this.pushRequiredStatus();
      }
    },
    inputValue(updatedValue, previousValue){
      if(updatedValue !== previousValue){
        this.pushRequiredStatus();
      }
    },
  },
};
</script>

<style scoped>
  :not(.input-field--error).input-required {
    background-color: rgba(255,230,21,.3);
  }
</style>
