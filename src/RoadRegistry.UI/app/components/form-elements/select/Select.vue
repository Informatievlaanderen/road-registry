<template>
  <select :name="name" :id="id" :class="classes" :disabled="disabled">
    <template v-for="(item, index) in options">
      <template v-if="item.type === 'option'">
        <option :value="item.value" :key="index" :selected="item.selected">{{ item.label }}</option>
      </template>
      <template v-if="item.type === 'optgroup'">
        <optgroup :label="item.label">
          <option v-for="(option, index) in item.options" :value="option.value" :key="index" :selected="option.selected">{{ option.label }}</option>
        </optgroup>
      </template>
    </template>
  </select>
</template>

<script>
export default {
  name: 'select',
  props: {
    name: {
      default: '',
      type: String
    },
    value: {
      default: '',
      type: String
    },
    id: {
      default: '',
      type: String
    },
    disabled: {
      default: false,
      type: Boolean
    },
    options: {
      type: Array
    },
    modIsBlock: {
      default: false,
      type: Boolean
    },
    modIsError: {
      default: false,
      type: Boolean
    },
    modShowOnSelect: {
      default: false,
      type: Boolean
    }
  },
  data () {
    return {
      classes: {
        'select': true,
        'select--block': this.modIsBlock,
        'select--error': this.modIsError,
        'select--disabled': this.disabled
      }
    }
  },
  mounted () {
    vl.select.showOnSelect.dressAll()
  }
}
</script>
