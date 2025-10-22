<script setup lang="ts">
import { ref } from 'vue';

const emit = defineEmits(['close-search']);

const searchQuery = ref('');

const closePopup = () => {
    emit('close-search');
};

const performSearch = () => {
    if (searchQuery.value.trim() === '') {
        // Optionally handle empty search query
        console.log('Search query is empty.');
        return;
    }
    console.log('Performing search for:', searchQuery.value);
    // Here you would typically navigate to a search results page
    // or emit an event to a parent component to handle the search.
    searchQuery.value = ''; // Clear search query after search
    closePopup(); // Close popup after search
};
</script>

<template>
    <!--Search Popup-->
    <div id="search-popup" class="search-popup">
        <div class="close-search theme-btn" @click="closePopup"><i class="far fa-times"></i></div>
        <div class="popup-inner">
            <div class="overlay-layer"></div>
            <div class="search-form">
                <form @submit.prevent="performSearch">
                    <div class="form-group">
                        <fieldset>
                            <input type="search" class="form-control" name="search-input" v-model="searchQuery"
                                placeholder="Type & Enter" required>
                        </fieldset>
                    </div>
                </form>
            </div>
        </div>
    </div>
</template>