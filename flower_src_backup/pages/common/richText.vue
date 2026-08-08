<template>
	<view class="page">
		<up-parse :content="data" :tagStyle="{img:'width:100%;display:block',video:'width:100%;display:block'}"></up-parse>
	</view>
</template>

<script>
	export default {
		data() {
			return {
				data:""
			}
		},
		onLoad(options) {
			this.request('Other/GetConfig',{queryType:options.type}).then(res=>{
				this.data = res.data
			})
		},
		methods: {
			
		}  
	}
</script>

<style lang="scss" scoped>
.page{
	padding:  0 32rpx;
}
</style>
