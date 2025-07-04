<template>
    <Navbar></Navbar>
    <div :class="['mv-container', showVotes ? '' : 'centered']">
        <div :class="['mv-panel', showVotes ? 'expanded' : 'collapsed']">
            <h2 class="text-2xl font-bold mb-4 text-center">Manage Votes</h2>
            <div class="flex flex-col gap-2">
                <button @click="onCreateVoteClicked" type="button"
                    class="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600">
                    Create Vote
                </button>
                <button @click="onShowVotesClicked" type="button"
                    class="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600">
                    {{ showVotes ? 'Hide votes' : 'Show all votes' }}
                </button>
            </div>

            <div v-if="showVotes && filterOptions" class="flex justify-end mt-4 mb-2 relative z-5">
                <button @click="onFilterClicked"
                    class="bg-gray-100 text-gray-800 font-semibold py-2 px-4 rounded shadow hover:bg-gray-200 flex items-center gap-2">
                    <span>Filter</span>
                    <span :class="filterPanelExpanded ? '' : 'rotate-90'" class="transition-transform">▼</span>
                </button>
            </div>

            <transition name="fade">
                <div v-if="filterPanelExpanded && showVotes && filterOptions"
                    class="fixed inset-10 flex items-start justify-center z-15"
                    @click.self="onFilterClicked">
                    <div class="mv-filter-overlay bg-white rounded-lg shadow-lg p-6 mt-24 w-full max-w-xl mx-auto relative"
                        @click.stop>
                        <div class="mv-filter-content flex flex-col md:flex-row gap-2 md:items-end">
                            <div class="flex flex-col" v-show="filterOptions.sortBy">
                                <label class="text-xs font-medium mb-1">Sort By</label>
                                <select name="sort-by" v-model="filterOptions.sortBy.value"
                                    class="border rounded px-2 py-1">
                                    <option value="" disabled>Select sort by</option>
                                    <option v-for="item in filterOptions.sortBy.options" :key="item.name"
                                        :value="item.name">{{ item.normalizedName }} </option>
                                </select>
                            </div>
                            <div class="flex flex-col" v-show="filterOptions.sortOrder">
                                <label class="text-xs font-medium mb-1">Sort Order</label>
                                <select v-model="filterOptions.sortOrder.value" class="border rounded px-2 py-1">
                                    <option value="" disabled>Select sort order</option>
                                    <option v-for="item in filterOptions.sortOrder.options" :key="item.name"
                                        :value="item.name">{{ item.normalizedName }}</option>
                                </select>
                            </div>
                            <div class="flex flex-col flex-1" v-show="filterOptions.search">
                                <label class="text-xs font-medium mb-1">Search</label>
                                <input v-model="filterOptions.search.value" type="text"
                                    class="border rounded px-2 py-1 w-full"
                                    :placeholder="`Search vote by ${filterOptions.search.options.map(o => o.normalizedName).join(', ')}...`" />
                            </div>
                            <button @click="onApplyFilterClicked" :disabled="isLoading"
                                class="text-white px-4 py-2 rounded mt-2 md:mt-0 md:min-h-10 md:min-w-28"
                                :class="[isLoading ? 'cursor-not-allowed bg-blue-200' : 'bg-blue-500 hover:bg-blue-600']">
                                <div v-if="isLoading" class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mx-auto"></div>
                                <p v-else>Apply Filter</p> 
                            </button>
                        </div>
                    </div>
                </div>
            </transition>

            <div v-if="showVotes" class="mv-list">
                <div v-if="isLoading"></div>
                <div v-else-if="votes === null || !votes.length" class="text-center py-8 text-gray-600">
                    Error getting vote list
                </div>
                <div v-else-if="votes.length === 0" class="text-center py-8 text-gray-600">
                    No votes available
                </div>
                <div v-for="vote, index in votes" :key="vote.id"
                    :class="['mv-item', { selected: selectedVoteId === vote.id }]" @click="onVoteItemClicked(vote.id)">
                    <div class="flex flex-col md:flex-row md:justify-between md:items-center gap-2 mb-2">
                        <div class="flex flex-col">
                            <div class="flex flex-col md:flex-row gap-2">
                                <h3 class="text-lg font-bold">{{ index + 1 }} |</h3>
                                <div class="flex flex-col gap-1">
                                    <h3 class="text-lg font-semibold whitespace-normal">{{ vote.vote.title }}</h3>
                                    <button @click.stop="onVoteSubjectsClicked(vote.id)"
                                        class="mv-subject-count w-max self-start">
                                        {{ vote.vote.subjects.length }} subjects
                                    </button>
                                </div>
                            </div>
                        </div>
                        <div class="flex flex-col md:items-end gap-1 text-right">
                            <div class="text-xs text-gray-500">Created by: <span class="font-medium">{{ vote.user ? vote.user.email : '-'
                                    }}</span></div>
                            <div class="text-sm text-gray-600">Total votes: {{ vote.vote.totalCount }}</div>
                            <div v-if="vote.vote.maximumCount" class="text-sm text-gray-600">Maximum votes: {{
                                vote.vote.maximumCount }}</div>
                            <div v-if="vote.vote.expiredTime" class="text-sm text-gray-600">Close at: {{
                                formatDateTime(vote.vote.expiredTime) }}</div>
                        </div>
                    </div>

                    <div :class="['mv-subjects', { expanded: expandedSubjectsId === vote.vote.id }]">
                        <div v-for="subject in vote.vote.subjects" :key="subject.id" class="mv-subject">
                            <span>{{ subject.name }}</span>
                            <span class="text-gray-600">{{ subject.voteCount }} votes
                                ({{ calculatePercentage(subject.voteCount, vote.vote.totalCount) }}%)</span>
                        </div>
                    </div>

                    <div :class="['mv-actions', { visible: selectedVoteId === vote.vote.id }]">
                        <div class="flex gap-2">
                            <button @click.stop="onEditClicked(vote.vote)"
                                class="bg-blue-500 text-white px-4 py-1 rounded hover:bg-blue-600">
                                Edit
                            </button>
                            <button @click.stop="onDeleteClicked(vote.vote)"
                                class="bg-red-500 text-white px-4 py-1 rounded hover:bg-red-600">
                                Delete
                            </button>
                        </div>
                    </div>
                </div>
                <div v-if="isLoading" class="flex items-center justify-center py-8">
                    <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mr-3"></div>
                    <span class="text-gray-600">Loading votes...</span>
                </div>
                <div v-else class="flex flex-col" style="padding-left: 0.5rem; padding-right: 0.5rem;">
                    <button class="text-center font-semibold py-2 bg-gray-100 rounded mb-2 hover:bg-gray-200"
                        @click="onLoadMoreClicked">
                        Load More
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref } from 'vue'
import { spawnLoading } from './components/loading'
import Navbar from './components/Navbar.vue'
import { delay, calculatePercentage, formatDateTime } from './utils'
import { spawnResultPopup } from './components/resultPopup'
import { spawnComponent } from './components/componentSpawner'
import VoteForm from './components/VoteForm.vue'
import { createNewVote, getVotes, updateVote, deleteVote, getFilterOptions } from './vote'
import { getUser } from './access'

const VotePerPageCount = 10

const showVotes = ref(false)
const selectedVoteId = ref(null)
const expandedSubjectsId = ref(null)
const votes = ref([])
const isLoading = ref(false)
const filterPanelExpanded = ref(false)
const filterOptions = ref(null)

const lastFilterOptionsValues = {
    sortBy: null,
    sortOrder: null
}

let voteForm = null
let currentPage = 0

function onVoteItemClicked(id) {
    selectedVoteId.value = selectedVoteId.value === id ? null : id
}

function onVoteSubjectsClicked(id) {
    expandedSubjectsId.value = expandedSubjectsId.value === id ? null : id
}

async function refreshWithPageCount() {
    const result = await fetchVotes(0, currentPage > 0 ? (currentPage + 1) * VotePerPageCount : VotePerPageCount)
    if (result) {
        votes.value = result
        return
    }

    if (currentPage === 0) {
        votes.value = null
    }
}

async function fetchVoteCreator(vote) {
    if (vote.creatorId) {
        return getUser(vote.creatorId)
            .then(resolve => {
                if (!resolve.result) {
                    return null
                }

                return resolve.result
            })
    }

    return null
}

async function fetchVotes(page = 0, count = VotePerPageCount) {
    isLoading.value = true
    try {
        const result = await getVotes(page,
            count,
            filterOptions.value.sortBy.value,
            filterOptions.value.sortOrder.value,
            filterOptions.value.search.value,
        )
        if (result.errorMessage) {
            return null
        }
        
        const mapped = result.result.map(v => {
            const voteObj = { id: v.id, vote: v, user: ref(null) }
            fetchVoteCreator(v)
                .then(response => {
                    voteObj.user.value = response
                })
            return voteObj
        })

        return mapped
    } catch (error) {
        console.error('Failed to fetch votes:', error)
        spawnResultPopup({
            feedbackText: 'Failed to load votes',
            success: false
        })
        return null
    } finally {
        isLoading.value = false
    }
}

function onCreateVoteClicked() {
    voteForm = spawnComponent(VoteForm, {
        formTitle: "Create a new Vote",
        closeOnPositive: false,
        onPositive: proceedCreateVote
    }, { zIndex: 20 })
    voteForm.onDestroy.subscribe(() => voteForm = null)
}

async function proceedCreateVote(data) {
    if (!data) {
        console.log("no data return from vote create form")
        return
    }

    const loading = spawnLoading({ loadingText: "Creating new vote..." }, 20)
    const subjects = data.voteSubjects.reduce((acc, elm) => {
        acc.push(elm.name)
        return acc
    }, [])

    try {
        const createResult = await createNewVote(data.voteTitle, subjects, data.voteDuration, data.voteMaxCount)
        const success = createResult && !createResult.errorMessage
        let feedbackText = `Successfully created new vote ${data.voteTitle}`

        if (!success) {
            feedbackText = `Failed: ${createResult.errorMessage}`
        }

        spawnResultPopup({
            feedbackText,
            success
        })

        if (!success) {
            if (createResult.validationErrors) {
                voteForm.instance.invalidateForm(createResult.validationErrors)
            }
            return
        }

        if (voteForm && voteForm.destroy) {
            voteForm.destroy()
        }

        if (showVotes.value) {
            await refreshWithPageCount()
        }
    } catch (error) {
        spawnResultPopup({ feedbackText: "Error creating vote", success: false })
        console.error("Error happened while creating vote", error)
    } finally {
        loading.destroy()
    }
}

async function onShowVotesClicked() {
    showVotes.value = !showVotes.value

    if (showVotes.value) {
        const filterOptionsResult = await getFilterOptions()
        const existingOptions = Object.entries(lastFilterOptionsValues)

        filterOptions.value = Object.entries(filterOptionsResult.result).reduce((acc, elm) => {
            const value = ref("")
            const options = Object.entries(elm[1])
            const currentKey = elm[0]
            const existingOption = existingOptions.find(([key]) => key === currentKey)

            if (currentKey !== "search") {
                if (existingOption && !existingOption[1]) {
                    value.value = options[0][0]
                } else {
                    value.value = existingOption[1]
                }
            }

            acc[elm[0]] = { options: options.map(o => { return { name: o[0], normalizedName: o[1].normalizedName } }), value }
            return acc
        }, {})

        const result = await fetchVotes()
        votes.value = result ? result : null
    } else {
        selectedVoteId.value = null
        expandedSubjectsId.value = null
    }
}

function onEditClicked(vote) {
    voteForm = spawnComponent(VoteForm, {
        id: vote.id,
        formTitle: `Update vote ${vote.title}`,
        title: vote.title,
        subjects: vote.subjects.reduce((acc, elm) => {
            acc.push({ id: elm.id, name: elm.name })
            return acc
        }, []),
        duration: vote.duration ? vote.duration : 0,
        maxCount: vote.maximumCount ? vote.maximumCount : 0,
        closeOnPositive: false,
        positiveText: "Confirm Edit",
        onPositive: proceedEditVote
    })
    voteForm.onDestroy.subscribe(() => voteForm = null)
}

async function proceedEditVote(data) {
    if (!data) {
        console.log("no data return from vote update form")
        return
    }

    const loading = spawnLoading({ loadingText: "Updating vote..." }, 20);

    try {
        const result = await updateVote(data.voteId, data.voteTitle,
            data.voteSubjects, data.voteDuration, data.voteMaxCount)

        let feedbackText = "Successfully updated vote"
        const success = result && !result.errorMessage

        if (!success) {
            feedbackText = `Failed: ${result.errorMessage}`
        }

        spawnResultPopup({ feedbackText, success }, 21)

        if (!success) {
            if (result.validationErrors) {
                voteForm.instance.invalidateForm(result.validationErrors)
            }
            return
        }

        if (voteForm && voteForm.destroy) {
            voteForm.destroy()
        }
        
        const updatedVoteIndex = votes.value.findIndex(v => v.id === result.result.id)
        if (updatedVoteIndex !== -1) {
            const updateVoteObj = { id: result.result.id, vote: result.result, user: ref(null) }
            votes.value[updatedVoteIndex] = updateVoteObj
            fetchVoteCreator(result.result)
                .then(response => {
                    updateVoteObj.user.value = response
                })
        }
    } catch (error) {
        console.error(`Error happened while deleting vote ${data.voteId}`, error)
        spawnResultPopup({ feedbackText: "Error updating vote", success: false })
    } finally {
        loading.destroy()
    }
}

function onDeleteClicked(vote) {
    voteForm = spawnComponent(VoteForm, {
        id: vote.id,
        formTitle: `Are you sure want to DELETE this vote?`,
        title: vote.title,
        subjects: vote.subjects.reduce((acc, elm) => {
            acc.push({ id: elm.id, name: elm.name })
            return acc
        }, []),
        duration: vote.duration ? vote.duration : 0,
        maxCount: vote.maximumCount ? vote.maximumCount : 0,
        closeOnPositive: false,
        positiveText: "Confirm",
        onPositive: proceedDeleteVote,
        readonly: true
    })
    voteForm.onDestroy.subscribe(() => voteForm = null)
}

async function proceedDeleteVote(data) {
    if (!data) {
        console.log("no data return from vote update form")
        return
    }

    const loading = spawnLoading({ loadingText: "Deleting vote..." }, 20);

    try {
        const result = await deleteVote(data.voteId)

        const success = result && !result.errorMessage
        let feedbackText = "Successfully deleted vote"

        if (!success) {
            feedbackText = `Failed: ${result.errorMessage}`
        }

        spawnResultPopup({ feedbackText, success }, 21)

        if (!success) {
            return
        }

        if (voteForm.destroy) {
            voteForm.destroy()
        }

        const deletedVoteIndex = votes.value.findIndex(v => v.id === result.result)
        if (deletedVoteIndex !== -1) {
            votes.value.splice(deletedVoteIndex, 1)
        }
    } catch (error) {
        console.error(`Error happened while deleting vote ${data.voteId}`, error)
        spawnResultPopup({ feedbackText: "Error deleting vote", success: false }, "21")
    } finally {
        loading.destroy()
    }
}

function onFilterClicked() {
    filterPanelExpanded.value = !filterPanelExpanded.value
}

async function onApplyFilterClicked() {
    await refreshWithPageCount()
    lastFilterOptionsValues.sortBy = filterOptions.value.sortBy.value
    lastFilterOptionsValues.sortOrder = filterOptions.value.sortOrder.value
}

async function onLoadMoreClicked() {
    currentPage++
    const result = await fetchVotes(currentPage)
    if (result) {
        result.forEach((vote) => {
            const existingIndex = votes.value.findIndex((v) => v.vote.id === vote.id)

            if (existingIndex !== -1) {
                votes.value[existingIndex] = vote
                return
            }

            votes.value.push(vote)
        })
    }
}
</script>