<template>
	<view v-if="bannerObj['10004']">
		<view class="swiper">
			<up-swiper :list="data.goodsImgList" height="750rpx" indicator indicatorMode="dot" circular></up-swiper>
		</view>
		<view class="countdown">
			<image :src="bannerObj['10004'].imgUrl" mode="widthFix" style="width: 100%;"></image>
			<view class="box">
				<view class="price">￥{{ info.price }}</view>
				<view class="text">已付款，平台提供担保</view>
			</view>
			<view class="time">
				<view class="text">还剩</view>
				<view class="num">{{ countdown.days }}</view>
				<view class="num">{{ countdown.hours }}</view>
				<view class="num">{{ countdown.minutes }}</view>
				<view class="num">{{ countdown.seconds }}</view>
			</view>
		</view>
		<view class="squareList">
			<view class="square">
				<view class="prices">
					<view class="price">￥{{ info.price.toFixed(2) }}</view>
					<view class="texts" @click="href('/pages/common/richText?type=2')">
						<view class="text">接单须知</view>
						<image src="/static/icon21.png"></image>
					</view>
				</view>
				<view class="intro">{{ info.relatedDemand }}</view>
			</view>
			<view class="square">
				<view class="item">
					<view class="label">花店</view>
					<view class="right">
						<view class="text">{{ data.shopName }}</view>
					</view>
				</view>
				<view class="item">
					<view class="label">方式</view>
					<view class="right">
						<view class="text">{{ data.receivingTypeName }}</view>
					</view>
				</view>
				<view class="item">
					<view class="label">时间</view>
					<view class="right">
						<view class="text">{{ info.receivingTime }}</view>
					</view>
				</view>
				<view class="item">
					<view class="label">地址</view>
					<view class="right">
						<view class="text">{{ info.province + info.city + info.area + info.address }}</view>
					</view>
				</view>
				<view class="item">
					<view class="label">备注</view>
					<view class="right">
						<view class="text">{{ info.remarks }}</view>
					</view>
				</view>
			</view>
		</view>
		<view style="height: 100rpx;"></view>
		<!-- 底部按钮 -->
		<view class="btnBox">
			<view class="btnBuy" @click="$u.throttle(clickApply,1000)">{{data.isApply? '已申请':'申请接单'}}</view>
		</view>
	</view>
</template>

<script>
export default {
	data() {
		return {
			data: {
				goodsImgList: []
			},
			info: {
				price: 0
			},
			id: '',
			bannerObj: {},
			countdown: {
				days: '00',
				hours: '00',
				minutes: '00',
				seconds: '00'
			},
			timer: null
		}
	},
	onLoad(options) {
		this.id = options.id
		this.getGoodsInfo()

	},
	onUnmounted() {
		this.stopCountdown() // 组件卸载时停止倒计时
	},
	onShow() {
		this.storeUser.updateUser()
		// 图片  
		this.request(`news/BannerList`, {
			Ids: [10004]
		}).then(res => {
			this.bannerObj = res.data.items
		})
	},
	methods: {
		// 申请接单
		clickApply() {
			if (this.data.isApply) {
				this.modal('提示', '您已申请接单，无需重复申请')
				return
			}
			this.request('Order/ApplyTask', {
				orderNo: this.info.orderNo
			}).then(res => {
				this.modal('提示', '申请接单成功')
				this.getGoodsInfo()
			})
		},
		async getGoodsInfo() {
			let res = await this.request('Order/TaskDetails', {
				queryId: this.id
			})
			this.data = res.data
			this.info = res.data.info
			this.updateCountdown() // 立即执行一次
			this.startCountdown()
		},
		previewImages(urls, index) {
			uni.previewImage({
				urls, // 图片路径数组
				current: index // 当前显示的图片索引
			});
		},
		toConfirm() {
			if (!this.isLogin()) {
				this.modal('登录', '进行登录操作', callback => {
					if (callback) {
						uni.navigateTo({
							url: '/pages/login/index'
						})
					}
				}, true, '暂不登录', '前往登录')
			} else {
			}
		},
		// 开始倒计时
		startCountdown() {
			this.timer = setInterval(() => {
				this.updateCountdown()
			}, 1000)
		},
		// 更新倒计时显示
		updateCountdown() {
			const now = new Date().getTime()
			const target = new Date(this.info.receivingTime).getTime()
			const distance = target - now
			if (distance <= 0) {
				// 倒计时结束
				this.countdown = {
					days: '00',
					hours: '00',
					minutes: '00',
					seconds: '00'
				}
				this.stopCountdown()
				return
			}

			// 计算天时分秒
			const days = Math.floor(distance / (1000 * 60 * 60 * 24))
			const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60))
			const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60))
			const seconds = Math.floor((distance % (1000 * 60)) / 1000)

			// 格式化显示（补零）
			this.countdown = {
				days: days.toString().padStart(2, '0') + '天',
				hours: hours.toString().padStart(2, '0') + '时',  
				minutes: minutes.toString().padStart(2, '0') + '分',
				seconds: seconds.toString().padStart(2, '0') + '秒'
			}
		},
		// 停止倒计时
		stopCountdown() {
			if (this.timer) {
				clearInterval(this.timer)
				this.timer = null
			}
		}
	},
	onShareAppMessage() {
		return {
			title: this.data.name,
			path: `/pagesShop/task/detail?id=${this.id}`
		}
	}
}
</script>

<style lang="scss" scoped>
.swiper {
	width: 750rpx;
	height: 750rpx;

}

.countdown {
	position: relative;
	overflow: hidden;

	.box {
		position: absolute;
		left: 0;
		top: 0;
		width: 750rpx;
		height: 100%;
		padding: 0 38rpx;
		display: flex;
		flex-direction: column;
		justify-content: center;

		.price {
			font-weight: 600;
			font-size: 40rpx;
			color: #FFFFFF;
		}

		.text {
			margin-top: 6rpx;
			font-size: 24rpx;
			color: #FFFFFF;
		}
	}

	.time {
		position: absolute;
		right: 45.5rpx;
		bottom: 15.5rpx;
		display: flex;
		align-items: center;
		.text{
			font-size: 20rpx;
			color: #FFFFFF;
			padding-right: 10rpx;
			white-space: nowrap;
		}
		.num{
			font-weight: 600;
			font-size: 18rpx;
			color: #8C4FFF;
			display: flex;
			align-items: center;
			justify-content: center;
			position: relative;
			background: white;
			padding: 6rpx;
			border-radius: 4rpx;
		}
		.num:not(:last-of-type) {
			margin-right: 10rpx;
		}
		.num:not(:last-of-type):before {
			position: absolute;
			content: ':';
			right: -8rpx;
			top: 50%;
			color: white;
			transform: translateY(-50%);
		}
	}
}

.squareList {
	padding: 24rpx 28rpx;

	.square {
		background: #FFFFFF;
		border-radius: 16rpx;
		padding: 24rpx;
		margin-bottom: 24rpx;

		.prices {
			display: flex;
			justify-content: space-between;

			.price {
				font-weight: 600;
				font-size: 40rpx;
				color: #DE4343;
			}

			.texts {
				display: flex;
				align-items: center;

				.text {
					font-size: 24rpx;
					color: #666666;
					white-space: nowrap;
				}

				image {
					margin-left: 10rpx;
					width: 30rpx;
					height: 30rpx;
				}
			}
		}

		.intro {
			margin-top: 16rpx;
			font-weight: 600;
			font-size: 32rpx;
			color: #333333;
		}

		.item {
			display: flex;
			padding: 30rpx 0;

			.label {
				font-size: 26rpx;
				color: #666666;
				white-space: nowrap;
			}

			.right {
				margin-left: 24rpx;
				width: 100%;
				font-size: 26rpx;
				color: #333333;
				display: flex;
				justify-content: space-between;
			}
		}

		.item:not(:last-child) {
			border-bottom: 2rpx solid #E2E2E2;
		}
	}
}

.btnBox {
	position: fixed;
	left: 0;
	bottom: 0;
	width: 100%;
	display: flex;
	align-items: center;
	justify-content: space-between;
	padding: 22rpx 32rpx;
	background: #FFFFFF;
	box-shadow: 0rpx 0rpx 16rpx 1rpx rgba(9, 4, 114, 0.2);

	.icons {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		margin-right: 32rpx;

		.name {
			font-size: 22rpx;
			color: #666666;
		}
	}

	.btnCar {
		white-space: nowrap;
		font-size: 28rpx;
		color: #333333;
		border-radius: 16rpx;
		border: 2rpx solid #333;
		padding: 22rpx 32rpx;
		text-align: center;
		margin-right: 32rpx;
	}

	.btnBuy {
		flex: 1;
		font-size: 28rpx;
		color: #FFFFFF;
		background: #8C4FFF;
		border-radius: 16rpx;
		padding: 22rpx 44rpx;
		text-align: center;
	}
}
</style>