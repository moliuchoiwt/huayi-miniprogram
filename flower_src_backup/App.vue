<script>
	import tui from '/utils/httpRequest'
	export default {
		onLaunch: () => {
			console.log('App Launch')
			// 获取静态资源路径
			tui.request('Other/GetConfig').then(res => {
				tui.itfImgUrl = res.data.domianStaticName
			})
		},
		onShow: function() {
			console.log('App Show')
			const updateManager = uni.getUpdateManager();

			updateManager.onCheckForUpdate(function(res) {
				if (res.hasUpdate) {
					updateManager.onUpdateReady(function(res2) {
						uni.showModal({
							title: '更新提示',
							content: '发现新版本，是否重启应用?',  
							cancelColor: '#eeeeee',
							confirmColor: '#FF0000',
							success(res2) {
								if (res2.confirm) {
									// 新的版本已经下载好，调用 applyUpdate 应用新版本并重启
									updateManager.applyUpdate();
								}
							}
						});
					});
				}
			});

			updateManager.onUpdateFailed(function(res) {
				// 新的版本下载失败
				uni.showModal({
					title: '提示',
					content: '检查到有新版本，但下载失败，请检查网络设置',
					success(res) {
						if (res.confirm) {
							// 新的版本已经下载好，调用 applyUpdate 应用新版本并重启
							updateManager.applyUpdate();
						}
					}
				});
			});
		},
		onHide: function() {
			console.log('App Hide')
		}
	}
</script>

<style lang="scss">
	/*每个页面公共css */
	@import "@/uni_modules/uview-plus/index.scss";

	page {
		background: #f6f6f6;
	}

	view {
		box-sizing: border-box;
	}

	image {
		display: block;
	}

	button {
		padding: 0;
		background-color: initial;
		margin: 0;
		line-height: initial;
		overflow: initial;
		box-sizing: initial;
		font-size: initial;
		border: 0;

	}

	button::after {
		content: none;
	}

	.uploadBox {
		width: 200rpx;
		height: 200rpx;
		background: #f8f8f8;
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;

		image {
			width: 56rpx;
			height: 46rpx;
		}

		.text {
			margin-top: 32rpx;
			font-size: 24rpx;
			color: #999999;
		}
	}

	.empty {
		padding-top: 20vh;
		display: flex;
		justify-content: center;

		image {
			width: 260rpx;
		}
	}

	.emptyBlock {
		padding-top: 32rpx;
		display: flex;
		justify-content: center;

		image {
			width: 260rpx;
		}
	}

	.tag {
		padding: 7rpx 14rpx;
		background: #B3915C;
		font-size: 24rpx;
		color: #fff;
		border-radius: 10rpx;
		margin-left: 10rpx;
	}

	.uGradeBox {
		margin-top: 16rpx;
		border-radius: 8rpx;
		display: flex;
		white-space: nowrap;
		overflow: hidden;

		.text {
			font-size: 22rpx;
			color: #FFFFFF;
			background: #78a254;
			padding: 6rpx;
		}

		.price {
			padding: 6rpx;
			font-size: 22rpx;
			color: #333333;
			background: #FCE4BE;
		}
	}
</style>