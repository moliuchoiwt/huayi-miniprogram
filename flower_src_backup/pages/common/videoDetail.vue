<template>
	<view class="page">
		<up-navbar title=" " bgColor='#ffffff00' leftIconColor='#fff' autoBack />
		<view class="videoBox" v-if="data.Url">
			<video :style="{'height':750 * videoHeight / videoWidth +'rpx'} " :src="data.Url"
				enable-play-gesture></video>
		</view>
		<view class="title">
			{{data.Title}}
		</view>
	</view>
</template>

<script>
	export default {
		data() {
			return {
				data: {},
				videoWidth: 0,
				videoHeight: 0,
			}
		},
		onLoad(options) {
			this.request('Article/ArticleInfo', {
				queryId: options.id
			}).then(res => {
				this.data = res.data.info
				this.videoWidth = res.data.videoWidth
				this.videoHeight = res.data.videoHeight
			})
			console.log(this.data);
		}
	}
</script>

<style lang="scss" scoped>
	.page {
		background: #000000;
		width: 100%;
		height: 100vh;
		color: white;

		video {
			width: 750rpx;
			max-height: 80vh;
		}

		.videoBox {
			display: flex;
			justify-content: center;
			align-items: center;
			width: 100%;
			height: 100vh;
		}

		.videoBox2 {
			display: flex;
			width: 100%;
			height: 100vh;
		}

		.title {
			position: fixed;
			left: 0;
			bottom: 94rpx;
			width: 100%;
			padding: 0 32rpx;
		}
	}
</style>